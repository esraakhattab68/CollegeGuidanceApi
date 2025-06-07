using CollegeGuideApi.DTOs.Authentication.Requests;
using CollegeGuideApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using CollegeGuideApi.DTOs.Authentication.Responses;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace CollegeGuideApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        #region Register

        [HttpPost("register")]
        public async Task<IActionResult> Register(SignUpRequestDTO dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDTO model)
        {
            try
            {
                var result = await _authService.VerifyOtpAsync(model.Email, model.Otp);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        #endregion

        #region Login
        [HttpPost("login")]


        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        #endregion

        #region External Login
        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _authService.ExternalLoginAsync(request);
            if (!response.Success)
            {

                return BadRequest(response);
            }
            return Ok(response);
        }
        #endregion

        #region Forget Password
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)] // يجب أن يكون string
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid request data.", GetModelStateErrors()));
            }

            _logger.LogInformation("Forgot password request received for email: {Email}", dto.Email);
            var response = await _authService.ForgotPasswordAsync(dto); 

            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("verifyOtpForgetPassword")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtpAsync([FromBody] VerifyOtpRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid request data.", GetModelStateErrors()));
            }

            _logger.LogInformation("Verify OTP request received for email: {Email}", dto.Email);

            var response = await _authService.VerifyOtpAsync(dto);

            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid request data.", GetModelStateErrors()));
            }

            _logger.LogInformation("Reset password request received."); 
            var response = await _authService.ResetPasswordAsync(dto);

            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }


        #endregion

        #region Change Password
        [HttpPost("change-password")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO requestDto)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);


            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User identity could not be determined from the token.");
            }

            var result = await _authService.ChangePasswordAsync(userEmail, requestDto);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message, Errors = result.Errors });
            }

            return Ok(new { Message = result.Message });
        }
        #endregion

        #region Edit Profile
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID claim (NameIdentifier) not found in token.");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {

                throw new InvalidOperationException($"User ID claim '{userIdClaim}' is not a valid integer.");
            }
            return userId;
        }

        [HttpGet("my-profile")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var response = await _authService.GetUserProfileAsync(userId);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPut("edit-name")]
        [Authorize]
        public async Task<IActionResult> UpdateNames([FromBody] UpdateProfileNamesRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = GetCurrentUserId();
            var response = await _authService.UpdateProfileNamesAsync(userId, dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("request-email-change")]
        [Authorize]
        public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = GetCurrentUserId();
            var response = await _authService.RequestEmailChangeAsync(userId, dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("confirm-email-change")]
        [Authorize]
        public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = GetCurrentUserId();
            var response = await _authService.ConfirmEmailChangeAsync(userId, dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("change-phoneNumber")]
        [Authorize]
        public async Task<IActionResult> ChangePhoneNumber([FromBody] ChangePhoneNumberRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = GetCurrentUserId();
            var response = await _authService.ChangePhoneNumberAsync(userId, dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        #endregion

        #region Support
        [HttpPost("send-message")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] ContactSupportDto contactDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("SupportController: Invalid model state for send-message.");
                return BadRequest(ModelState);
            }

            try
            {
                await _authService.SendSupportInquiryEmailAsync(contactDto, cancellationToken);
                _logger.LogInformation($"SupportController: Support message from {contactDto.Email} processed and sent successfully.");
                return Ok(new { message = "Your message has been sent successfully. We will get back to you soon." });
            }
            catch (System.ApplicationException appEx) // لالتقاط الأخطاء التي أعدنا رميها من MailService
            {
                _logger.LogError(appEx, $"SupportController: Application error processing support message from {contactDto.Email}.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"We encountered an issue: {appEx.Message}" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"SupportController: Unexpected error processing support message from {contactDto.Email}.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred. Please try again later." });
            }
        }
        #endregion

        #region Logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var jtiClaim = User.FindFirst(JwtRegisteredClaimNames.Jti);
                if (jtiClaim == null || string.IsNullOrWhiteSpace(jtiClaim.Value))
                {
                    _logger.LogWarning("Logout attempt: Token missing JTI claim. User: {UserIdentifier}", User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown");
                    // حتى لو لم يكن هناك JTI، لا يزال العميل يجب أن يحذف التوكن.
                    // السيرفر لا يمكنه إبطال التوكن بدون JTI.
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Logout processed (client-side action required). Server-side revocation skipped due to missing JTI."));
                }

                var jti = jtiClaim.Value;
                DateTime tokenExpirationDate;

                var expClaim = User.FindFirst("exp"); // 'exp' هو Unix timestamp
                if (expClaim != null && long.TryParse(expClaim.Value, out long expUnixTimestamp))
                {
                    tokenExpirationDate = DateTimeOffset.FromUnixTimeSeconds(expUnixTimestamp).UtcDateTime;
                    _logger.LogInformation("Token JTI '{Jti}' for user '{UserId}' will be revoked. Original expiration from 'exp' claim: {ExpirationDate}", jti, User.FindFirstValue(ClaimTypes.NameIdentifier), tokenExpirationDate);
                }
                else
                {
                    // كحل بديل إذا لم يتم العثور على 'exp' claim، استخدم مدة صلاحية التوكن من الإعدادات
                    var jwtSettings = _configuration.GetSection("Jwt");
                    double defaultExpirationMinutes = 60; // قيمة افتراضية
                    if (double.TryParse(jwtSettings["AccessTokenExpirationMinutes"], out double configuredMinutes) && configuredMinutes > 0)
                    {
                        defaultExpirationMinutes = configuredMinutes;
                    }
                    tokenExpirationDate = DateTime.UtcNow.AddMinutes(defaultExpirationMinutes);
                    _logger.LogWarning("Token JTI '{Jti}' for user '{UserId}': 'exp' claim not found or invalid. Using calculated expiration: {CalculatedExpirationDate}. Pruning is important.", jti, User.FindFirstValue(ClaimTypes.NameIdentifier), tokenExpirationDate);
                }

                await _authService.RevokeTokenAsync(jti, tokenExpirationDate);

                return Ok(ApiResponse<object>.SuccessResponse(null, "Successfully logged out. Token has been scheduled for revocation."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the logout process for user {UserId}.", User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown");
                // من المهم إخبار العميل بأن تسجيل الخروج يجب أن يتم من جهته على أي حال.
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.FailureResponse("An error occurred on the server during logout. Please ensure you clear the token on the client-side."));
            }
        }
        #endregion
    }
}
