using System;
using System.Collections.Generic;
using System.Linq;

namespace SerousCommonLib.API.Iterators {
	/// <summary>
	/// An iterator that randomly iterates through each item of an <see cref="IEnumerable{TSource}"/>
	/// </summary>
	/// <typeparam name="TSource">The type of the source enumerable.</typeparam>
	public class RandomEnumerableIterator<TSource> : Iterator<TSource> {
		private readonly IEnumerable<TSource> _source;
		private readonly Func<int, int> _getRandomIndex;
		private readonly bool _allowRepetitions;
		private readonly HashSet<int> _iteratedIndices;
		private TSource[] _sourceAsArray;

		public RandomEnumerableIterator(IEnumerable<TSource> source, Func<int, int> getRandomIndex, bool allowRepetitions) {
			ArgumentNullException.ThrowIfNull(source);
			ArgumentNullException.ThrowIfNull(getRandomIndex);
			_source = source;
			_getRandomIndex = getRandomIndex;
			_allowRepetitions = allowRepetitions;
			_iteratedIndices = new();
		}

		public override Iterator<TSource> Clone() => new RandomEnumerableIterator<TSource>(_source, _getRandomIndex, _allowRepetitions);

		public override bool MoveNext() {
			switch (_state) {
				case 1:
					_sourceAsArray = _source.ToArray();
					_state = 2;
					goto case 2;
				case 2:
					int index = GetNextIndex();

					if (index >= 0) {
						_current = _sourceAsArray[index];
						return true;
					}

					Dispose();
					break;
			}

			return false;
		}

		private int GetNextIndex() {
			int rand;
			bool alreadyExisted;

			if (_iteratedIndices.Count == _sourceAsArray.Length)
				return -1;

			do {
				rand = _getRandomIndex(_sourceAsArray.Length);
				alreadyExisted = !_iteratedIndices.Add(rand);
			} while (!_allowRepetitions && alreadyExisted);

			return rand;
		}
	}
}
