internal enum State
{
    None,
    ShowAllVacancies,
    SearchEmployer
}


internal class UserState
{
    public State State { get; set; }
}