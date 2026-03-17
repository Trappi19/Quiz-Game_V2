[System.Serializable]
public class RoleData
{
    public int id;
    public string name;
    public string description;

    public RoleData(int id, string name, string description)
    {
        this.id = id;
        this.name = name;
        this.description = description;
    }
}
