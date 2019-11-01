namespace Git.Net
{
    public static partial class Git
    {
        public readonly struct ExitCode
        {
            private readonly int _code;

            public ExitCode(int code)
                => _code = code;
            
            public static implicit operator bool(ExitCode code)
                => code._code == 0;
            
            public static implicit operator ExitCode(int code)
                => new ExitCode(code);
            
            public override string ToString() 
                => $"Exit Code: {_code}";
        }
    }
}
