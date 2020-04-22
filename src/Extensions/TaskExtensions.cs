using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Extensions {
	public static class TaskExtensions {

		public static async Task HandleTimeout(this Task task, TimeSpan timeout) {

			var taskCompletionSource = new TaskCompletionSource<bool>();
			
			using var cancellationToken = new CancellationTokenSource();
			cancellationToken.CancelAfter(timeout);
			cancellationToken.Token.Register(() => taskCompletionSource.TrySetCanceled(), false);
			
			var cancellationTask = taskCompletionSource.Task;

			var mergedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);
			
			if(mergedTask == cancellationTask)
				await task.ContinueWith(_ => task.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(false);

			await mergedTask.ConfigureAwait(false);
		}
		
		public static async Task<TResult> HandleTimeout<TResult>(this Task<TResult> task, TimeSpan timeout) {

			var taskCompletionSource = new TaskCompletionSource<TResult>();
			
			using var cancellationToken = new CancellationTokenSource();
			cancellationToken.CancelAfter(timeout);
			cancellationToken.Token.Register(() => taskCompletionSource.TrySetCanceled(), false);
			
			var cancellationTask = taskCompletionSource.Task;

			var mergedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);
			
			if(mergedTask == cancellationTask)
				await task.ContinueWith(_ => task.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(false);

			return await mergedTask.ConfigureAwait(false);
		}

		public static Task<T> WithAllExceptions<T>(this Task<T> task) {
			var tcs = new TaskCompletionSource<T>();

			var task2 = task.ContinueWith(ignored => {
				switch(task.Status) {
					case TaskStatus.Canceled:
						tcs.SetCanceled();

						break;

					case TaskStatus.RanToCompletion:
						// ReSharper disable once AsyncConverter.AsyncWait
						tcs.SetResult(task.Result);

						break;

					case TaskStatus.Faulted:

						// SetException will automatically wrap the original AggregateException
						// in another one. The new wrapper will be removed in TaskAwaiter, leaving
						// the original intact.
						tcs.SetException(task.Exception);

						break;

					default:
						tcs.SetException(new InvalidOperationException("Continuation called illegally."));

						break;
				}
			});

			return tcs.Task;
		}
	}
}