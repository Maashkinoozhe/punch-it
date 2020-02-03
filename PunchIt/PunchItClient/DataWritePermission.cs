namespace PunchItClient
{
    internal class DataWritePermission
    {
        public DataWritePermission(bool updateState, bool updateProject, bool updateRecord)
        {
            UpdateState = updateState;
            UpdateProject = updateProject;
            UpdateRecord = updateRecord;
        }

        public bool UpdateState { get; set; }
        public bool UpdateProject  {get;set;}
        public bool UpdateRecord { get; set; }
    }
}