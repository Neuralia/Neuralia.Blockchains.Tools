using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Extensions {
	public static class TaskExtensions {

		public static async Task<bool> HandleTimeout(this Task task, TimeSpan timeout) {

			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

			using CancellationTokenSource cancellationToken = new CancellationTokenSource();
			cancellationToken.CancelAfter(timeout);
			cancellationToken.Token.Register(() => taskCompletionSource.TrySetCanceled(), false);

			Task<bool> cancellationTask = taskCompletionSource.Task;

			Task mergedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);

			bool cancelled = mergedTask == cancellationTask;
			if(!cancelled) {
				// await if there were any exception
				await task.ConfigureAwait(false);
			}
			
			return !cancelled;
		}

		public static async Task<(TResult result, bool success)> HandleTimeout<TResult>(this Task<TResult> task, TimeSpan timeout) {

			TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();

			using CancellationTokenSource cancellationToken = new CancellationTokenSource();
			cancellationToken.CancelAfter(timeout);
			cancellationToken.Token.Register(() => taskCompletionSource.TrySetCanceled(), false);

			Task<TResult> cancellationTask = taskCompletionSource.Task;

			Task<TResult> mergedTask = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);

			if(mergedTask == cancellationTask) {
				// we timed out
				var t = task.ContinueWith(_ => task.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
			}

			bool cancelled = mergedTask == cancellationTask;
			if(!cancelled) {
				// await if there were any exception
				await task.ConfigureAwait(false);
			}

			return (cancelled?default:mergedTask.Result, !cancelled);
		}

		public static Task<T> WithAllExceptions<T>(this Task<T> task) {
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

			Task task2 = task.ContinueWith(ignored => {
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