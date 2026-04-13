public static class RoleEffectFactory
{
    public static IRoleEffect Create(int roleId)
    {
        switch (roleId)
        {
            case 1:
                return new DebuggerRoleEffect();
            default:
                return new NoRoleEffect();
        }
    }
}
