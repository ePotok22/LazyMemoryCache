using System.Runtime.CompilerServices;

namespace FFF.LazyMemoryCache
{
    /// <summary>
    /// Provides support for asynchronous lazy initialization. This class is useful when the initialization
    /// of a resource is expensive and should be deferred until it is actually needed, and the initialization
    /// itself is asynchronous.
    /// </summary>
    /// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class for lazy initialization
        /// of a value using a delegate that returns a value of type T.
        /// </summary>
        /// <param name="valueFactory">A delegate that returns a value to be used for lazy initialization.
        /// This delegate is executed in a thread-safe manner the first time the Lazy<T> object is accessed.
        /// The delegate is run on the ThreadPool.</param>
        public AsyncLazy(Func<T> valueFactory)
            : base(() => Task.Factory.StartNew(valueFactory))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class for lazy initialization
        /// of a value using a task-returning delegate that returns a value of type T.
        /// </summary>
        /// <param name="taskFactory">A delegate that returns a Task<T> to be used for lazy initialization.
        /// The delegate is executed in a thread-safe manner the first time the Lazy<Task<T>> object is accessed.
        /// This overload ensures that the task is started as soon as the value is required.</param>
        public AsyncLazy(Func<Task<T>> taskFactory)
            : base(() => Task.Factory.StartNew(taskFactory).Unwrap())
        {
        }

        /// <summary>
        /// Provides an awaiter for the asynchronous lazy initialization. This method allows the
        /// <see cref="AsyncLazy{T}"/> instance to be awaited directly, facilitating easy integration
        /// with asynchronous code.
        /// </summary>
        /// <returns>Returns a TaskAwaiter<T> that can be used to await this Lazy<Task<T>>.</returns>
        public TaskAwaiter<T> GetAwaiter() =>
            Value.GetAwaiter();
    }
}
