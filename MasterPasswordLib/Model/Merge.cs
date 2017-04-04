using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterPassword.Model
{
    /// <summary>
    /// Helper class to merge two configurations. Considers only additions and that
    /// </summary>
    public class Merge
    {
        /// <summary>
        /// the Merge result
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Name of the user, is used to generate the passwords for the sites
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// Merged site entries including new ones and conflicts
            /// </summary>
            public List<MergedEntry> SitesMerged { get; private set; }

            public Result(string userName)
            {
                UserName = userName;
                SitesMerged = new List<MergedEntry>();
            }
        }

        /// <summary>
        /// Merged site entry
        /// </summary>
        public class MergedEntry
        {
            /// <summary>
            /// can be null
            /// </summary>
            public SiteEntry First { get; private set; }

            /// <summary>
            /// can be null
            /// </summary>
            public SiteEntry Second { get; private set; }

            /// <summary>
            /// Which one to use by default (if both are same then first is picked)
            /// </summary>
            public Resolution Which { get; set; }

            public enum Resolution
            {
                /// <summary>
                /// Both the same
                /// </summary>
                Identical,

                /// <summary>
                /// Use First
                /// </summary>
                FirstNew,
                FirstNewer,

                /// <summary>
                /// Use Second
                /// </summary>
                SecondNew,
                SecondNewer,

                /// <summary>
                /// Unclear which is better, please decide
                /// </summary>
                Conflict
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="first">first (can be null if second is not null)</param>
            /// <param name="second">second (can be null if first is not null)</param>
            /// <param name="which">recommended resolution</param>
            public MergedEntry(SiteEntry first, SiteEntry second, Resolution which)
            {
                if (first == null && second == null) throw new ArgumentException("both are not allowed to be null");

                First = first;
                Second = second;
                Which = which;
            }
        }

        /// <summary>
        /// Perform simple merge. UserName must be same in both configurations already.
        /// </summary>
        /// <param name="firstCfg">first ("our") configuration</param>
        /// <param name="secondCfg">second ("remote") configuration</param>
        /// <returns>merge result</returns>
        public static Result Perform(Configuration firstCfg, Configuration secondCfg)
        {
            if (firstCfg.UserName != secondCfg.UserName)
            {
                throw new ArgumentException("Configurations are not compatible, they use different user names ('" + firstCfg.UserName + "' != '" + secondCfg.UserName + "')");
            }

            var result = new Result(firstCfg.UserName);

            // we only consider additions, not deletes.
            var left = firstCfg.Sites.ToList();
            var right = secondCfg.Sites.ToList();

            // first: find entries with the same site names
            while (left.Count > 0)
            {
                var first = left[0];
                left.RemoveAt(0);

                SiteEntry second = null;

                // find partner in right
                foreach (var item in right)
                {
                    if (item.SiteName == first.SiteName)
                    {   // found partner.
                        second = item;
                        right.Remove(item);
                        break;
                    }
                }

                if (null == second)
                {   // no partner? we're new in the original and just stay as is
                    result.SitesMerged.Add(new MergedEntry(first, null, MergedEntry.Resolution.FirstNew));
                }
                else if (first.Type == second.Type && first.Counter == second.Counter && first.Login == second.Login && first.SiteName == second.SiteName && first.Type == second.Type)
                {   // all the same
                    result.SitesMerged.Add(new MergedEntry(first, second, MergedEntry.Resolution.Identical));
                }
                else
                {   // potential conflict - exists in both sites
                    // bigger counter? that one wins
                    if (first.Type == second.Type && first.Counter > second.Counter)
                    {
                        result.SitesMerged.Add(new MergedEntry(first, second, MergedEntry.Resolution.FirstNewer));
                    }
                    else if (first.Type == second.Type && first.Counter < second.Counter)
                    {
                        result.SitesMerged.Add(new MergedEntry(first, second, MergedEntry.Resolution.SecondNewer));       
                    }
                    else
                    {   // same counter - conflict. try our best.
                        if (first.Type == second.Type && first.Login.StartsWith(second.Login))
                        {   // all the same but login is better in first
                            result.SitesMerged.Add(new MergedEntry(first, second, MergedEntry.Resolution.FirstNewer));
                        }
                        else if (first.Type == second.Type && second.Login.StartsWith(first.Login))
                        {   // all the same but login is better in second
                            result.SitesMerged.Add(new MergedEntry(first, second, MergedEntry.Resolution.SecondNewer));
                        }
                        else
                        {   // I have no idea
                            result.SitesMerged.Add(new MergedEntry(first, second, MergedEntry.Resolution.Conflict));
                        }
                    }
                }
            }

            // remaining in right are new there
            foreach (var item in right)
            {
                result.SitesMerged.Add(new MergedEntry(null, item, MergedEntry.Resolution.SecondNew));
            }

            return result;
        }
    }
}
