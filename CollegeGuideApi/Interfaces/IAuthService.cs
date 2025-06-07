using CollegeGuideApi.DTOs.Authentication.Requests;
using CollegeGuideApi.DTOs.Authentication.Responses;

namespace CollegeGuideApi.Interfaces
{
    public interface IAuthService
    {
        #region Register
        Task<ApiResponse<string>> RegisterAsync(SignUpRequestDTO dto);
        Task<ApiResponse<string>> VerifyOtpAsync(string email, string otp);
        #endregion

        #region Login
        Task<ApiResponse<string>> LoginAsync(LoginRequestDTO loginRequestDTO);
        #endregion

        #region External Login
        Task<ApiResponse<string>> ExternalLoginAsync(ExternalLoginRequestDTO externalLoginRequestDTO);

        #endregion

        #region Forget Password
        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDTO dto);
        Task<ApiResponse<string>> VerifyOtpAsync(VerifyOtpRequestDTO dto);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDTO dto);
        #endregion

        #region Change Password
        Task<ApiResponse<string>> ChangePasswordAsync(string userEmail, ChangePasswordRequestDTO dto);

        #endregion

        #region Edit Profile
        Task<ApiResponse<UserProfileResponseDTO>> GetUserProfileAsync(int userId);
        Task<ApiResponse<UserProfileResponseDTO>> UpdateProfileNamesAsync(int userId, UpdateProfileNamesRequestDTO dto);
        Task<ApiResponse<string>> RequestEmailChangeAsync(int userId, RequestEmailChangeRequestDTO dto);
        Task<ApiResponse<string>> ConfirmEmailChangeAsync(int userId, ConfirmEmailChangeRequestDTO dto);
        Task<ApiResponse<string>> ChangePhoneNumberAsync(int userId, ChangePhoneNumberRequestDTO dto);
        #endregion

        #region Support
        Task SendSupportInquiryEmailAsync(ContactSupportDto supportDetails, CancellationToken cancellationToken = default);
        #endregion

        #region Logout
        Task RevokeTokenAsync(string jti, DateTime expirationDate);
        Task<bool> IsTokenRevokedAsync(string jti);
        Task PruneExpiredTokensAsync();
        #endregion
    }
}
