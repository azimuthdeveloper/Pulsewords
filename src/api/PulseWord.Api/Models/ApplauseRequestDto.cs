using System;

namespace PulseWord.Api.Models
{
    /// <summary>
    /// Request DTO for applauding a user.
    /// </summary>
    public class ApplauseRequestDto
    {
        /// <summary>
        /// The ID of the user to applaud.
        /// </summary>
        public Guid ToUserId { get; set; }
    }
}
