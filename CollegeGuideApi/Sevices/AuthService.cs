using CollegeGuideApi.DTOs.Authentication.Requests;
using CollegeGuideApi.DTOs.Authentication.Responses;
using CollegeGuideApi.Helpers;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.DbContexts;
using CollegeGuideApi.Models.Entities;
using Google.Apis.Auth;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using IMailService = CollegeGuideApi.Interfaces.IMailService;


namespace CollegeGuideApi.Sevices
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMailService mailService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _dataProtector;
        private readonly IWebHostEnvironment _env;

        #region ForgetPassword
        private const string PasswordResetTokenPurpose = "PasswordResetPurpose";
        private const int OtpExpirationMinutes = 15;
        private const int ResetTokenExpirationMinutes = 10;
        #endregion
        public AuthService(ApplicationDbContext context,IMailService mailService, ITokenService tokenService, ILogger<AuthService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _context = context;
            this.mailService = mailService;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _dataProtector = dataProtectionProvider.CreateProtector(PasswordResetTokenPurpose);
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        #region Register
        public async Task<ApiResponse<string>> RegisterAsync(SignUpRequestDTO dto)
        {
            if (dto.Password != dto.Password)
                return ApiResponse<string>.FailureResponse("Passwords do not match.");

            var allowedUserTypes = new[] { "Student", "Parent" };
            if (!allowedUserTypes.Contains(dto.UserType))
                return ApiResponse<string>.FailureResponse("Invalid user type.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<string>.FailureResponse("Email is already registered.");

            var user = new ApplicationUser
            {
                FName = dto.FirstName,
                LName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UserType = dto.UserType,
                IsExternalLogin = false,
                EmailConfirmed = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var otp = GenerateOtp();

            var existing = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == dto.Email);
            if (existing != null)
                _context.EmailVerifications.Remove(existing);

            _context.EmailVerifications.Add(new EmailVerification
            {
                Email = dto.Email,
                Otp = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)
            });

            await _context.SaveChangesAsync();
            var htmlBody = File.ReadAllText("Templates/EmailOtpTemplate.html").Replace("{{OTP}}", otp);

            await mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = dto.Email,
                Subject = "Verification Code",
                Body = htmlBody
            });

            return ApiResponse<string>.SuccessResponse("Verify your Email. Enter OTP sent to your Email.");
        }
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // OTP مكون من 6 أرقام
        }
        #endregion

        #region Verify Otp[register]
        public async Task<ApiResponse<string>> VerifyOtpAsync(string email, string otp)
        {
            var verification = await _context.EmailVerifications.FirstOrDefaultAsync(e => e.Email == email && e.Otp == otp);
            if (verification == null || verification.ExpirationTime < DateTime.UtcNow)
                return ApiResponse<string>.FailureResponse("Invalid or expired OTP.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return ApiResponse<string>.FailureResponse("User not found.");

            user.EmailConfirmed = true;
            _context.EmailVerifications.Remove(verification);

            var userEntry = _context.Entry(user);

            var verificationEntry = _context.Entry(verification);

            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);
            return ApiResponse<string>.SuccessResponse(
            message: "Your Email has been verified successfully!",
            token: token                                 
            );

        }
        #endregion

        #region Login
        public async Task<ApiResponse<string>> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            if (loginRequestDTO == null || string.IsNullOrWhiteSpace(loginRequestDTO.Email) || string.IsNullOrWhiteSpace(loginRequestDTO.Password))
            {
                return ApiResponse<string>.FailureResponse("Email and password are required.");
            }

            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Email == loginRequestDTO.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(loginRequestDTO.Password, user.PasswordHash))
            {
                return ApiResponse<string>.FailureResponse("Invalid email or password.");
            }

            var token = _tokenService.GenerateToken(user);
            return ApiResponse<string>.SuccessResponse(
           message: "Login Successfully!.",
           token: token
           );
        }

        #endregion

        #region External Login
        public async Task<ApiResponse<string>> ExternalLoginAsync(ExternalLoginRequestDTO request)
        {
            _logger.LogInformation("External login attempt for provider: {Provider}", request.Provider);

            ExternalLoginUserInfo externalUser;

            if (string.Equals(request.Provider, "Google", StringComparison.OrdinalIgnoreCase))
            {
                externalUser = await VerifyGoogleTokenAsync(request.Token);
            }
            else if (string.Equals(request.Provider, "Facebook", StringComparison.OrdinalIgnoreCase))
            {
                externalUser = await VerifyFacebookTokenAsync(request.Token);
            }
            else
            {
                _logger.LogWarning("Unsupported external login provider: {Provider}", request.Provider);
                return ApiResponse<string>.FailureResponse("Unsupported external login provider.");
            }

            if (externalUser == null || string.IsNullOrEmpty(externalUser.Email) || string.IsNullOrEmpty(externalUser.ProviderUserId))
            {
                _logger.LogError("Failed to verify external token or retrieve user info for provider {Provider}.", request.Provider);
                return ApiResponse<string>.FailureResponse("Invalid external token or failed to retrieve user information.");
            }

            var existingExternalLogin = await _context.ExternalLogins
                .Include(el => el.User) 
                .FirstOrDefaultAsync(el => el.Provider == request.Provider && el.ProviderUserId == externalUser.ProviderUserId);

            ApplicationUser appUser;

            if (existingExternalLogin != null)
            {
                _logger.LogInformation("External login found for Provider: {Provider}, ProviderUserId: {ProviderUserId}. UserID: {UserId}",
                    request.Provider, externalUser.ProviderUserId, existingExternalLogin.User.Id);
                appUser = existingExternalLogin.User;
                if (appUser == null)
                {
                    _logger.LogError("External login record exists but associated ApplicationUser is null. Provider: {Provider}, ProviderUserId: {ProviderUserId}", request.Provider, externalUser.ProviderUserId);
                    return ApiResponse<string>.FailureResponse("User account associated with this external login is missing or corrupted.");
                }
            }
            else
            {
                appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == externalUser.Email);

                if (appUser != null)
                {
                    _logger.LogInformation("Local user found with email {Email}. Linking external login Provider: {Provider}, ProviderUserId: {ProviderUserId}",
                        externalUser.Email, request.Provider, externalUser.ProviderUserId);

                    var newExternalLogin = new ExternalLogin
                    {
                        Provider = request.Provider,
                        ProviderUserId = externalUser.ProviderUserId,
                        UserId = appUser.Id
                    };
                    _context.ExternalLogins.Add(newExternalLogin);

                    if (!appUser.IsExternalLogin) appUser.IsExternalLogin = true;
                    if (!appUser.EmailConfirmed) appUser.EmailConfirmed = true; 
                }
                else
                {
                    _logger.LogInformation("No existing user found. Creating new user for external login. Email: {Email}, Provider: {Provider}",
                        externalUser.Email, request.Provider);
                    
                    string userType = "Student"; 
                    if (!string.IsNullOrWhiteSpace(request.UserType) && (request.UserType == "Student" || request.UserType == "Parent"))
                    {
                        userType = request.UserType;
                    }
                    else if (string.IsNullOrWhiteSpace(request.UserType))
                    {
                        _logger.LogWarning("UserType not provided for new external user {Email}. Defaulting to 'Student'. Client should ideally provide this or user should complete profile.", externalUser.Email);
                       
                    }
                    else
                    {
                        _logger.LogWarning("Invalid UserType '{UserType}' provided for new external user {Email}. Defaulting to 'Student'.", request.UserType, externalUser.Email);
                    }


                    appUser = new ApplicationUser
                    {
                        FName = externalUser.FirstName,
                        LName = externalUser.LastName,
                        Email = externalUser.Email,
                        UserType = userType, 
                        IsExternalLogin = true,
                        EmailConfirmed = true, 
                        PasswordHash = null, 
                        PhoneNumber = externalUser.PhoneNumber 
                    };
                    _context.Users.Add(appUser);

                    await _context.SaveChangesAsync(); 

                    var newExternalLogin = new ExternalLogin
                    {
                        Provider = request.Provider,
                        ProviderUserId = externalUser.ProviderUserId,
                        UserId = appUser.Id 
                    };
                    _context.ExternalLogins.Add(newExternalLogin);
                }
                await _context.SaveChangesAsync(); 
            }

            var token = _tokenService.GenerateToken(appUser);
            return ApiResponse<string>.SuccessResponse(
            message: "External login successful!.",
            token: token
            );
        }

        private async Task<ExternalLoginUserInfo> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];
                if (string.IsNullOrEmpty(googleClientId))
                {
                    _logger.LogError("Google ClientId is not configured in appsettings.json.");
                    return null;
                }

                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { googleClientId }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new ExternalLoginUserInfo
                {
                    ProviderUserId = payload.Subject,
                    Email = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    // Google doesn't reliably provide phone number in id_token
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid Google ID token: {IdToken}", idToken);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying Google ID token.");
                return null;
            }
        }

        private async Task<ExternalLoginUserInfo> VerifyFacebookTokenAsync(string accessToken)
        {
            try
            {
                //var fbAppId = _configuration["Authentication:Facebook:AppId"];
                //var fbAppSecret = _configuration["Authentication:Facebook:AppSecret"]; // For app access token if needed, but user token is fine for /me

                var httpClient = _httpClientFactory.CreateClient();
                // Request basic fields: id, name, email, first_name, last_name
                // Note: Facebook may not return email if user hasn't granted permission or doesn't have a primary email.
                var response = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,name,email,first_name,last_name&access_token={accessToken}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Facebook Graph API error: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Facebook Graph API response: {Content}", content);

                var fbUser = JObject.Parse(content);

                var email = fbUser.Value<string>("email");
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Facebook user {FbId} did not return an email address.", fbUser.Value<string>("id"));
                    // Decide how to handle this. For now, we'll fail if email is missing.
                    // You could allow login without email if your system supports it, or ask user to provide one.
                    return null;
                }


                return new ExternalLoginUserInfo
                {
                    ProviderUserId = fbUser.Value<string>("id"),
                    Email = email,
                    FirstName = fbUser.Value<string>("first_name"),
                    LastName = fbUser.Value<string>("last_name"),
                    // Facebook usually doesn't provide phone number via basic /me endpoint
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while verifying Facebook token.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying Facebook access token.");
                return null;
            }
        }
        #endregion

        #region Forget Password 
        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return ApiResponse<string>.FailureResponse("Email address is required.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

          
            if (user != null)
            {
                var otp = GenerateOtp();

                var existingOtpRecords = _context.EmailVerifications
                    .Where(e => e.Email == dto.Email && !e.IsUsed);
                if (existingOtpRecords.Any())
                {
                    _context.EmailVerifications.RemoveRange(existingOtpRecords);
                }

                _context.EmailVerifications.Add(new EmailVerification
                {
                    Email = dto.Email,
                    Otp = otp,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(OtpExpirationMinutes),
                    IsUsed = false
                });

                await _context.SaveChangesAsync();

                string htmlBody;
                string passwordResetTemplatePath = "Templates/PasswordResetOtpTemplate.html";
                string fullTemplatePath = Path.GetFullPath(passwordResetTemplatePath);

                if (File.Exists(passwordResetTemplatePath))
                {
                    htmlBody = await File.ReadAllTextAsync(passwordResetTemplatePath);
                    htmlBody = htmlBody.Replace("{{OTP}}", otp);
                }
                else
                {
                    htmlBody = $"<p>Your One-Time Password (OTP) for password reset is: <strong>{otp}</strong></p><p>This OTP is valid for 15 minutes.</p>";
                    Console.WriteLine($"Warning: PasswordResetOtpTemplate.html not found at {fullTemplatePath}. Using default email body.");
                }

                await mailService.SendEmailAsync(new MailRequest
                {
                    ToEmail = dto.Email,
                    Subject = "Password Reset OTP",
                    Body = htmlBody
                });
            }
            return ApiResponse<string>.SuccessResponse("Verify your Email. Enter OTP sent to your Email.");
        }
        #endregion

        #region Verify OTP[forget password] 
        public async Task<ApiResponse<string>> VerifyOtpAsync(VerifyOtpRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return ApiResponse<string>.FailureResponse("Email address is required.");
            }
            if (string.IsNullOrWhiteSpace(dto.Otp))
            {
                return ApiResponse<string>.FailureResponse("OTP is required.");
            }

            var verificationRecord = await _context.EmailVerifications
                .FirstOrDefaultAsync(e => e.Email == dto.Email && e.Otp == dto.Otp && !e.IsUsed);

            if (verificationRecord == null)
            {
                return ApiResponse<string>.FailureResponse("Invalid or already used OTP.");
            }

            if (verificationRecord.ExpirationTime < DateTime.UtcNow)
            {
                _context.EmailVerifications.Remove(verificationRecord);
                await _context.SaveChangesAsync();
                return ApiResponse<string>.FailureResponse("Expired OTP. Please request a new one.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                _context.EmailVerifications.Remove(verificationRecord);
                await _context.SaveChangesAsync();
                return ApiResponse<string>.FailureResponse("User associated with this OTP not found.");
            }

            verificationRecord.IsUsed = true;
            
            await _context.SaveChangesAsync();

          
            var tokenPayload = $"{dto.Email}|{DateTime.UtcNow.AddMinutes(ResetTokenExpirationMinutes):O}"; 
            var resetToken = _dataProtector.Protect(tokenPayload);

            return ApiResponse<string>.SuccessResponse(
             message: "OTP verified successfully. Use the provided token to reset your password.",
             token: resetToken
         );
        }
        #endregion

        #region Reset Password 
        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return ApiResponse<string>.FailureResponse("Email is required.");
            }
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
            {
                return ApiResponse<string>.FailureResponse("New password and confirmation password are required.");
            }

            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return ApiResponse<string>.FailureResponse("New passwords do not match.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                
                return ApiResponse<string>.FailureResponse("An error occurred. Please try again or contact support if the issue persists.");
            }

            var usedOtpRecord = await _context.EmailVerifications
            .FirstOrDefaultAsync(ev => ev.Email == dto.Email && ev.IsUsed && ev.ExpirationTime > DateTime.UtcNow.AddMinutes(-OtpExpirationMinutes - 5)); // Check for OTPs marked as used recently

            if (usedOtpRecord != null)
            {
                _context.EmailVerifications.Remove(usedOtpRecord);
                _logger.LogInformation($"Removed used OTP record for email {dto.Email}.");
            }
            else
            {
             
                _logger.LogWarning($"No recently used OTP record found for email {dto.Email} during password reset. This might be okay if cleanup is handled elsewhere or OTP was not used.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while resetting password.");
                return ApiResponse<string>.FailureResponse("Could not save password due to a conflict. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving password reset to database.");
                return ApiResponse<string>.FailureResponse("An unexpected error occurred while saving your new password.");
            }

            return ApiResponse<string>.SuccessResponse("Your password has been reset successfully.");

        }
        #endregion

        #region Change Password
        public async Task<ApiResponse<string>> ChangePasswordAsync(string userEmail, ChangePasswordRequestDTO dto)
        {
            if (dto == null)
            {
                return ApiResponse<string>.FailureResponse("Request data is missing.");
            }
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                return ApiResponse<string>.FailureResponse("Current password is required.");
            }
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return ApiResponse<string>.FailureResponse("New password is required.");
            }
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return ApiResponse<string>.FailureResponse("New passwords do not match.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return ApiResponse<string>.FailureResponse("Invalid credentials provided.");
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse<string>.FailureResponse("Incorrect current password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            string notificationHtmlBody = $"<p>Dear {user.FName},</p><p>Your password was successfully changed on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}.</p><p>If you did not make this change, please contact our support team immediately.</p>";
            await mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = user.Email,
                Subject = "Password Change Notification",
                Body = notificationHtmlBody
            });

            return ApiResponse<string>.SuccessResponse("Password changed successfully.");
        }
        #endregion

        #region Edit Profile
        public async Task<ApiResponse<UserProfileResponseDTO>> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserProfileResponseDTO>.FailureResponse("User not found.");
            }

            var userProfile = new UserProfileResponseDTO
            {
                FName = user.FName,
                LName = user.LName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                UserType = user.UserType
            };

            string userProfileJson = JsonSerializer.Serialize(userProfile);

            return ApiResponse<UserProfileResponseDTO>.SuccessResponse(message: userProfileJson, token: null);
        }

        public async Task<ApiResponse<UserProfileResponseDTO>> UpdateProfileNamesAsync(int userId, UpdateProfileNamesRequestDTO dto)
        {
            var user = await _context.Users.FindAsync(userId); 
            if (user == null)
            {
                return ApiResponse<UserProfileResponseDTO>.FailureResponse("User not found.");
            }

            user.FName = dto.FName;
            user.LName = dto.LName;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var updatedProfile = new UserProfileResponseDTO
            {
                FName = user.FName,
                LName = user.LName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                UserType = user.UserType
            };

            string userProfileJson = JsonSerializer.Serialize(updatedProfile);

            return ApiResponse<UserProfileResponseDTO>.SuccessResponse(message: userProfileJson, token: null);
        }

        public async Task<ApiResponse<string>> RequestEmailChangeAsync(int userId, RequestEmailChangeRequestDTO dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<string>.FailureResponse("User not found.");
            }

            if (string.Equals(user.Email, dto.NewEmail, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<string>.FailureResponse("New email is the same as the current email.");
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.NewEmail && u.Id != userId);
            if (emailExists)
            {
                return ApiResponse<string>.FailureResponse("This email address is already in use by another account.");
            }

            var otp = GenerateOtp(); 

            var existingOtps = _context.EmailVerifications
                .Where(ev => ev.Email == dto.NewEmail && ev.Purpose == "EmailChange");
            _context.EmailVerifications.RemoveRange(existingOtps);

            _context.EmailVerifications.Add(new EmailVerification
            {
                Email = dto.NewEmail, 
                Otp = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10), 
                Purpose = "EmailChange", 
                UserId = userId 
            });

            await _context.SaveChangesAsync();

            var htmlBody = $"Your OTP to confirm new email address is: <strong>{otp}</strong>. This OTP is valid for 10 minutes.";
            htmlBody = File.ReadAllText("Templates/EmailOtpTemplate.html").Replace("{{OTP}}", otp);

            await mailService.SendEmailAsync(new MailRequest
            {
                ToEmail = dto.NewEmail, 
                Subject = "Confirm Your New Email Address",
                Body = htmlBody
            });

            return ApiResponse<string>.SuccessResponse("OTP sent to your new email address. Please verify to complete the change.");
        }

        public async Task<ApiResponse<string>> ConfirmEmailChangeAsync(int userIdPerformingChange, ConfirmEmailChangeRequestDTO dto)
        {
           
            var verification = await _context.EmailVerifications
                .FirstOrDefaultAsync(ev => ev.Email == dto.NewEmail &&
                                            ev.Otp == dto.Otp &&
                                            ev.Purpose == "EmailChange" &&
                                            ev.UserId == userIdPerformingChange); 

            if (verification == null)
            {
                return ApiResponse<string>.FailureResponse("Invalid OTP.");
            }

            if (verification.ExpirationTime < DateTime.UtcNow)
            {
                _context.EmailVerifications.Remove(verification);
                await _context.SaveChangesAsync();
                return ApiResponse<string>.FailureResponse("Expired OTP. Please request a new one.");
            }

            var userToUpdate = await _context.Users.FindAsync(userIdPerformingChange); 
            if (userToUpdate == null)
            {
                _context.EmailVerifications.Remove(verification);
                await _context.SaveChangesAsync();
                return ApiResponse<string>.FailureResponse("User not found for this email change request.");
            }

            userToUpdate.Email = dto.NewEmail;
            userToUpdate.EmailConfirmed = true; 

            _context.Users.Update(userToUpdate);
            _context.EmailVerifications.Remove(verification); 

            await _context.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse("Email address changed successfully.");
        }

        public async Task<ApiResponse<string>> ChangePhoneNumberAsync(int userId, ChangePhoneNumberRequestDTO dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<string>.FailureResponse("User not found.");
            }

          

            user.PhoneNumber = dto.NewPhoneNumber;
            user.PhoneNumberConfirmed = true; 

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse("Phone number updated successfully.");
        }

        #endregion

        #region Support
        public async Task SendSupportInquiryEmailAsync(ContactSupportDto supportDetails, CancellationToken cancellationToken = default)
        {
            var appSenderEmail = _configuration["MailSettings:Mail"];
            var appSenderDisplayName = _configuration["MailSettings:DisplayName"];
            var companySupportEmail = _configuration["MailSettings:CompanyMail"];

            if (string.IsNullOrEmpty(appSenderEmail) || string.IsNullOrEmpty(companySupportEmail) || string.IsNullOrEmpty(_configuration["MailSettings:Password"]))
            {
                _logger.LogError("Mail settings (Mail, CompanyMail, or Password) are not configured properly.");
                throw new System.InvalidOperationException("Mail settings are not configured properly for sending support emails.");
            }

            // --- قراءة محتوى القالب من الملف ---
            string templatePath = Path.Combine(_env.ContentRootPath, "Templates", "SupportRequestTemplate.html");
            string htmlTemplate;
            try
            {
                htmlTemplate = await File.ReadAllTextAsync(templatePath, cancellationToken);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, $"Email template file not found at path: {templatePath}");
                throw new System.ApplicationException("Support email template is missing.", ex);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error reading email template file at path: {templatePath}");
                throw new System.ApplicationException("Could not read support email template.", ex);
            }

            // --- تعبئة العلامات النائبة في القالب ---
            string logoUrl = _configuration["AppSettings:AppLogoUrl"]; // افترض أن لديك هذا في appsettings.json
            // أو يمكنك وضعه مباشرة: string logoUrl = "https://yourdomain.com/logo.png";
            // إذا لم يكن لديك شعار، اجعل logoUrl فارغًا أو null
            // string logoUrl = "https://via.placeholder.com/150x50.png?text=College+Guide+Logo"; // شعار مؤقت

            string logoImageTag = string.IsNullOrEmpty(logoUrl) ? "" : $"<img src=\"{logoUrl}\" alt=\"College Guide Logo\">";

            htmlTemplate = htmlTemplate.Replace("{{LogoImageTag}}", logoImageTag);
            htmlTemplate = htmlTemplate.Replace("{{UserName}}", WebUtility.HtmlEncode(supportDetails.Name));
            htmlTemplate = htmlTemplate.Replace("{{UserEmail}}", WebUtility.HtmlEncode(supportDetails.Email));
            // لرسالة المستخدم، تأكد من عمل encode ثم استبدال فواصل الأسطر
            string encodedMessage = WebUtility.HtmlEncode(supportDetails.Message).Replace("\n", "<br />");
            htmlTemplate = htmlTemplate.Replace("{{UserMessage}}", encodedMessage);
            htmlTemplate = htmlTemplate.Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());


            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(appSenderDisplayName ?? "College Guide", appSenderEmail));
            email.To.Add(MailboxAddress.Parse(companySupportEmail));
            email.Subject = $"New Support Request from: {supportDetails.Name}";
            email.ReplyTo.Add(new MailboxAddress(supportDetails.Name, supportDetails.Email));

            var builder = new BodyBuilder();
            builder.HtmlBody = htmlTemplate; // <--- استخدام القالب الذي تمت تعبئته
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Timeout = 30000;

            try
            {
                await smtp.ConnectAsync(
                    _configuration["MailSettings:Host"],
                    int.Parse(_configuration["MailSettings:Port"]),
                    SecureSocketOptions.StartTls,
                    cancellationToken);

                await smtp.AuthenticateAsync(
                    _configuration["MailSettings:Mail"],
                    _configuration["MailSettings:Password"],
                    cancellationToken);

                await smtp.SendAsync(email, cancellationToken);
                _logger.LogInformation($"Support inquiry from {supportDetails.Email} sent successfully to {companySupportEmail} using template.");
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, $"SMTP Command Error sending support inquiry from {supportDetails.Email}. Status Code: {ex.StatusCode}, Message: {ex.Message}");
                throw new System.ApplicationException($"Failed to send support email due to SMTP command error: {ex.Message} (Code: {ex.StatusCode})", ex);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"General error sending support inquiry from {supportDetails.Email}.");
                throw new System.ApplicationException($"An unexpected error occurred while sending the support email: {ex.Message}", ex);
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true, cancellationToken);
                }
            }
        }

        #endregion

        #region Logout
        public async Task RevokeTokenAsync(string jti, DateTime expirationDate)
        {
            if (string.IsNullOrWhiteSpace(jti))
            {
                _logger.LogWarning("Attempted to revoke a token with null or empty JTI.");
                return;
            }

            if (await _context.RevokedTokens.AnyAsync(rt => rt.Jti == jti))
            {
                _logger.LogInformation("Token with JTI '{Jti}' is already revoked. No action taken.", jti);
                return;
            }

            var revokedToken = new RevokedToken
            {
                Jti = jti,
                ExpirationDate = expirationDate
            };

            await _context.RevokedTokens.AddAsync(revokedToken);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Token with JTI '{Jti}' successfully revoked until {ExpirationDate}.", jti, expirationDate);
        }

        public async Task<bool> IsTokenRevokedAsync(string jti)
        {
            if (string.IsNullOrWhiteSpace(jti))
            {
                return false;
            }
            return await _context.RevokedTokens.AnyAsync(rt => rt.Jti == jti && rt.ExpirationDate > DateTime.UtcNow);
        }

        public async Task PruneExpiredTokensAsync()
        {
            var cutoffDate = DateTime.UtcNow;
            var tokensToRemove = await _context.RevokedTokens
                                             .Where(rt => rt.ExpirationDate <= cutoffDate)
                                             .ToListAsync();
            if (tokensToRemove.Any())
            {
                _context.RevokedTokens.RemoveRange(tokensToRemove);
                await _context.SaveChangesAsync();
                _logger.LogInformation("{Count} expired revoked token entries pruned from the denylist.", tokensToRemove.Count);
            }
            else
            {
                _logger.LogInformation("No expired revoked tokens to prune.");
            }
        }
        #endregion


    }
}
