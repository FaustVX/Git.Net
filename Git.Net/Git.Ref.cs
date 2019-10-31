using System;

namespace Git.Net
{
    public static partial class Git
    {
        public readonly struct Ref
        {
            private readonly Index _indexFrom;

            private readonly string _ref;
            private readonly bool _fromRef;

            private readonly int _sha1CommitInt;
            private readonly bool _fromSha1Int;

            private readonly ulong _sha1CommitULong;
            private readonly bool _fromSha1ULong;


        ///<summary>Use the '^1' form</summary>
        ///<param name="indexFromHEAD">Use the '^1' form</param>
            public Ref(Index indexFromHEAD)
                : this()
            {
                _indexFrom = indexFromHEAD;
            }


        ///<summary>Use the '^1' form</summary>
        ///<param name="indexFrom">Use the '^1' form</param>
        ///<param name="ref">tag/branch</param>
            public Ref(Index indexFrom, string @ref)
                : this(indexFrom)
            {
                _ref = @ref;
                _fromRef = true;
            }

        ///<summary>tag/branch</summary>
        ///<param name="ref">tag/branch</param>
            public Ref(string @ref)
                : this()
            {
                _ref = @ref;
                _fromRef = true;
            }

        ///<summary>Use the '0x14ab250' form, up to 0x7fffffff</summary>
        ///<param name="sha1Commit">Use the '0x14ab250' form, up to 0x7fffffff</param>
            public Ref(int sha1Commit)
                : this()
            {
                _sha1CommitInt = sha1Commit;
                _fromSha1Int = true;
            }


        ///<summary>Use the '0x14ab250' form, up to 0x7fffffff</summary>
        ///<param name="indexFrom">Use the '^1' form</param>
        ///<param name="sha1Commit">Use the '0x14ab250' form, up to 0x7fffffff</param>
            public Ref(Index indexFrom, int sha1Commit)
                : this(indexFrom)
            {
                _sha1CommitInt = sha1Commit;
                _fromSha1Int = true;
            }

        ///<summary>Use the '0x14ab250uL' form, up to 16 digits</summary>
        ///<param name="sha1Commit">Use the '0x14ab250uL' form, up to 16 digits</param>
            public Ref(ulong sha1Commit)
                : this()
            {
                _sha1CommitULong = sha1Commit;
                _fromSha1ULong = true;
            }

        ///<summary>Use the '0x14ab250uL' form, up to 16 digits</summary>
        ///<param name="indexFrom">Use the '^1' form</param>
        ///<param name="sha1Commit">Use the '0x14ab250uL' form, up to 16 digits</param>
            public Ref(Index indexFrom, ulong sha1Commit)
                : this(indexFrom)
            {
                _sha1CommitULong = sha1Commit;
                _fromSha1ULong = true;
            }


            public override string ToString()
                => (_fromRef, _fromSha1Int, _fromSha1ULong) switch
                {
                    (false, false, false) => "HEAD",
                    (true, false, false) => _ref,
                    (false, true, false) => $"{_sha1CommitInt:x}",
                    (false, false, true) => $"{_sha1CommitULong:x}",
                } + (_indexFrom is { Value: var offset } && offset > 0 ? $"~{offset}" : "");
            
            
            public static implicit operator string(Ref @ref)
                => @ref.ToString();
            
            public static implicit operator Ref(Index indexFromHEAD)
                => new Ref(indexFromHEAD);
            
            public static implicit operator Ref(int sha1Commit)
                => new Ref(sha1Commit);
            
            public static implicit operator Ref(ulong sha1Commit)
                => new Ref(sha1Commit);
            
            public static implicit operator Ref(string @ref)
                => new Ref(@ref);
        }
    }
}
