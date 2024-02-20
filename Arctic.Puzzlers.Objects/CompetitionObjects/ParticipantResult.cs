﻿namespace Arctic.Puzzlers.Objects.CompetitionObjects
{
    public class ParticipantResult
    {
        public ParticipantResult() 
        {
            Participants = new List<Participant>();
            Results = new List<Result>();
        }
        public List<Participant> Participants { get; set; }
        public List<Result> Results { get; set; }
        
    }
}