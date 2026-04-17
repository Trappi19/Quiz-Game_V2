public static class RoleEffectFactory
{
    public static IRoleEffect Create(int roleId)
    {
        switch (roleId)
        {
            case 1:
                return new DebuggerRoleEffect();
            case 2:
                return new HackerRoleEffect();
            case 3:
                return new CompilateurRoleEffect();
            case 4:
                return new CacheRoleEffect();
            case 5:
                return new DevOpsRoleEffect();
            case 6:
                return new MultiLanguageRoleEffect();
            case 9:
                return new FullstackRoleEffect();
            default:
                return new NoRoleEffect();
        }
    }
}
