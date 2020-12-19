using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Neuralia.Blockchains.Tools.Threading {
    /// <summary>
    ///     a special class that creates an async/await aware parallel foreach
    /// </summary>
    public static class ParallelAsync {
		public static async Task ForEach<T>(IEnumerable<T> collection, Func<(T entry, int index), Task> lambda, int? maxDegreeOfParallelism = null) {
			if(collection == null) {
				return;
			}

			T[] entries = collection.ToArray();

			if(!maxDegreeOfParallelism.HasValue) {
				maxDegreeOfParallelism = Math.Max(Environment.ProcessorCount / 2, 1);
			}

			ActionBlock<(T entry, int index)> transformBlock = new ActionBlock<(T entry, int index)>(lambda, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism.Value});

			int index = 0;

			foreach(T entry in entries) {
				await transformBlock.SendAsync((entry, index)).ConfigureAwait(false);
				index++;
			}

			transformBlock.Complete();
			await transformBlock.Completion.ConfigureAwait(false);
		}
	}
}