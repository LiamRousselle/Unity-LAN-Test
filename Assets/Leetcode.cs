public class Solution {
    private string result;

    private bool doesStrPrefixMatch(string[] strs, string subscribed) {
        foreach (string iteration in strs) {
            string checkSub = iteration.Substring(0, subscribed.Length);
            if (checkSub != subscribed) {
                return false;
            }
        }
        return true;
    }

    public string LongestCommonPrefix(string[] strs) {
        if (strs.Length == 1) {
            return strs[0];
        }

        foreach (string iteration in strs) {
            for (int i = 0; i < iteration.Length; i++) {
                string subscribed = iteration.Substring(0, i);
                if (string.IsNullOrEmpty(subscribed)) {
                    continue;
                }

                if (doesStrPrefixMatch(strs, subscribed)) {
                    result = subscribed;
                    continue;
                }

                break;
            }
        }

        return result;
    }
}