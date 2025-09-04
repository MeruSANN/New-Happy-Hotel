namespace HappyHotel.Buff
{
    public enum BuffMergeType
    {
        Coexist, // 共存（当前行为）
        Replace, // 新Buff替换旧Buff
        Merge, // 合并到现有Buff中
        Reject // 拒绝新Buff
    }

    public class BuffMergeResult
    {
        private BuffMergeResult(BuffMergeType mergeType, BuffBase resultBuff, string reason)
        {
            MergeType = mergeType;
            ResultBuff = resultBuff;
            Reason = reason;
        }

        public BuffMergeType MergeType { get; private set; }
        public BuffBase ResultBuff { get; private set; } // 合并后的Buff（如果适用）
        public string Reason { get; private set; } // 合并原因说明

        public static BuffMergeResult CreateCoexist()
        {
            return new BuffMergeResult(BuffMergeType.Coexist, null, "允许共存");
        }

        public static BuffMergeResult CreateReplace(BuffBase newBuff)
        {
            return new BuffMergeResult(BuffMergeType.Replace, newBuff, "新Buff替换旧Buff");
        }

        public static BuffMergeResult CreateMerge(BuffBase mergedBuff)
        {
            return new BuffMergeResult(BuffMergeType.Merge, mergedBuff, "合并到现有Buff");
        }

        public static BuffMergeResult CreateReject(string reason)
        {
            return new BuffMergeResult(BuffMergeType.Reject, null, reason);
        }
    }
}