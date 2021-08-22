using System.Text.Json.Serialization;

namespace ScanServer_NetCore.Services.Enums
{
    /// <summary>
    /// Predefined scan quality options
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScanQuality
    {
        /// <summary>
        /// Fastest but also least good scan quality
        /// </summary>
        Fast = 300, 
        /// <summary>
        /// Normal, should fit most purposes
        /// </summary>
        Normal = 450,
        /// <summary>
        /// Better than normal, will take slightly longer
        /// </summary>
        Good = 600,
        /// <summary>
        /// Best quality so far, will take its time
        /// </summary>
        Best = 750, // 4 * 150 + 150
    }
}
