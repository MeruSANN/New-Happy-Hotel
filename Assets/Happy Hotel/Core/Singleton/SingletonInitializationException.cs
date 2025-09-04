using System;

namespace HappyHotel.Core.Singleton
{
    /// <summary>
    ///     单例初始化异常
    /// </summary>
    public class SingletonInitializationException : Exception
    {
        public enum InitializationErrorType
        {
            CircularDependency, // 循环依赖
            MissingDependency, // 缺少依赖
            InitializationFailed, // 初始化失败
            InvalidDependency // 无效依赖
        }

        public SingletonInitializationException(Type problemType, InitializationErrorType errorType,
            string message = null)
            : base(FormatMessage(problemType, errorType, message))
        {
            ProblemType = problemType;
            ErrorType = errorType;
        }

        public Type ProblemType { get; }
        public InitializationErrorType ErrorType { get; }

        private static string FormatMessage(Type problemType, InitializationErrorType errorType,
            string additionalMessage)
        {
            var baseMessage = errorType switch
            {
                InitializationErrorType.CircularDependency =>
                    $"检测到循环依赖: {problemType.Name}",
                InitializationErrorType.MissingDependency =>
                    $"缺少必需的依赖: {problemType.Name}",
                InitializationErrorType.InitializationFailed =>
                    $"初始化失败: {problemType.Name}",
                InitializationErrorType.InvalidDependency =>
                    $"无效的依赖: {problemType.Name}",
                _ => $"未知错误: {problemType.Name}"
            };

            return string.IsNullOrEmpty(additionalMessage)
                ? baseMessage
                : $"{baseMessage} - {additionalMessage}";
        }
    }
}