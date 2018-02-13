using System;
namespace Happimeter.Core.Helper
{
    public static class UtilHelper
    {
        /// <summary>
        /// Gets the major minor from user identifier.
        /// if userId higher than 65535, than we increment major by one.
        /// </summary>
        /// <returns>The major minor from user identifier.</returns>
        /// <param name="userId">User identifier.</param>
        public static (int, int) GetMajorMinorFromUserId(int userId) {
            int major = userId / 65535;
            int minor = userId % 65535;

            return (major, minor);
        }
    }
}
