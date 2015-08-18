using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMasterPassword.Common
{
    public class SynchronizeLists
    {
        /// <summary>
        /// Synchronize one list to another, trying to reduce reallocations, returning removed items
        /// </summary>
        /// <typeparam name="TSource">item type of source list</typeparam>
        /// <typeparam name="TDest">itme type if destination list</typeparam>
        /// <param name="dest">destination list, will be changed</param>
        /// <param name="source">source list, will be read (only once)</param>
        /// <param name="compare">decide if a source and a destination item "are equal" </param>
        /// <param name="factory">create item for destination list for a source item</param>
        /// <returns>removed items</returns>
        internal static List<TDest> Sync<TSource, TDest>(IList<TDest> dest, IEnumerable<TSource> source, Func<TSource, TDest, bool> compare, 
            Func<TSource, TDest> factory)
        {
            var removed = new List<TDest>();

            int currrentIndex = 0;

            foreach (var sourceItem in source)
            {
                if (dest.Count < currrentIndex + 1)
                {   // list not long enough -> add
                    dest.Add(factory(sourceItem));
                }
                else
                {   // list long enough - compare current item
                    if (!compare(sourceItem, dest[currrentIndex]))
                    {   // this is not the same -> remove current item
                        TDest newItem = default(TDest);

                        bool found = false;

                        // find in old removed ones
                        foreach (var removedItem in removed)
                        {
                            if (compare(sourceItem, removedItem))
                            {   // found
                                newItem = removedItem;
                                found = true;
                                removed.Remove(removedItem);
                                break;
                            }
                        }

                        if (!found)
                        {   // no new item yet 

                            // ROOM FOR IMPROVEMENTS
                            // - try to find in later of dest to avoid reallocations, (optionally using the ObservableCollection.Move)
                            newItem = factory(sourceItem);
                        }

                        removed.Add(dest[currrentIndex]);
                        dest[currrentIndex] = newItem;
                    }
                }

                currrentIndex++;
            }

            // remaining items at the end? remove them
            while (dest.Count > currrentIndex)
            {
                removed.Add(dest[currrentIndex]);
                dest.RemoveAt(currrentIndex);
            }

            return removed;
        }

        /// <summary>
        /// Synchronize one list to another, just overwriting the destination, always creating a new item, without much intelligence
        /// </summary>
        /// <typeparam name="TSource">item type of source list</typeparam>
        /// <typeparam name="TDest">itme type if destination list</typeparam>
        /// <param name="dest">destination list, will be changed</param>
        /// <param name="source">source list, will be read (only once)</param>
        /// <param name="factory">create item for destination list for a source item</param>
        /// <returns>removed items</returns>
        internal static void Sync<TSource, TDest>(IList<TDest> dest, IEnumerable<TSource> source, Func<TSource, TDest> factory)
        {
            int currrentIndex = 0;

            foreach (var sourceItem in source)
            {
                var newItem = factory(sourceItem);
                if (dest.Count < currrentIndex + 1)
                {   // list not long enough -> add
                    dest.Add(newItem);
                }
                else
                {   // list long enough - overwrite
                    dest[currrentIndex] = newItem;
                }

                currrentIndex++;
            }

            // remaining items at the end? remove them
            while (dest.Count > currrentIndex)
            {
                dest.RemoveAt(dest.Count - 1); // remove last
            }
        }
    }
}
