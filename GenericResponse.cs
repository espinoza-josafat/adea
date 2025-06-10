public partial class GenericResponse
{
    public GenericResponse()
    {
        Status = 1;
        Message = "OK";
        Description = "OK";
        Data = null;
    }

    public virtual int Status { get; set; }

    public virtual string Message { get; set; }

    public virtual string Description { get; set; }

    public virtual object Data { get; set; }
}

public partial class GenericResponse<T>
{
    public GenericResponse()
    {
        Status = 1;
        Message = "OK";
        Description = "OK";
        Data = default;
    }

    public virtual int Status { get; set; }

    public virtual string Message { get; set; }

    public virtual string Description { get; set; }

    public virtual T Data { get; set; }
}
