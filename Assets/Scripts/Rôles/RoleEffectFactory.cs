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
            case 9:
                return new FullstackRoleEffect();
            default:
                return new NoRoleEffect();
        }
    }
}
