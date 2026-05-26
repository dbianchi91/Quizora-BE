namespace AITutor.Domain.ValueObjects;

public record StudyPlanId(Guid Value)
{
    public static StudyPlanId New() => new(Guid.NewGuid());
    public static StudyPlanId From(Guid v) => new(v);
    public override string ToString() => Value.ToString();
}
