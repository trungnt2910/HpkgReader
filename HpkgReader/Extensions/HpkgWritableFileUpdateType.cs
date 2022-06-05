namespace HpkgReader.Extensions
{
    public enum HpkgWritableFileUpdateType: uint
    {
        /// <summary>
        /// the old file shall be kept
        /// </summary>
        KeepOld = 0,
        /// <summary>
        /// the old file needs to be updated manually
        /// </summary>
        Manual = 1,
        /// <summary>
        /// an automatic three-way merge shall be attempted
        /// </summary>
        AutoMerge = 2
    }
}
