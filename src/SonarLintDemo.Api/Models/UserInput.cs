namespace SonarLintDemo.Api.Models;

public class UserInput
{
    // S1104: Make this field 'private' and encapsulate it in a property.
    public string RawValue = string.Empty;

    // S3457: Format strings should be used correctly
    public string FormatMessage(string name)
    {
        return string.Format("User {0} logged in at {1}", name);
    }

    // S1117: Rename 'value' which hides the field with the same name.
    public void SetValue(string value)
    {
        var value2 = value;
        RawValue = value2;
    }
}
