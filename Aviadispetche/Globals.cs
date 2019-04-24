namespace Aviadispetcher
{
    public class Flight
    {
        public static int logUser { get; set; }
        public Flight(string nF, string cF, System.TimeSpan tF, System.TimeSpan At, int fS)
    {
        this.Number = nF;
        this.City = cF;
        this.Departure_time = tF;
        this.Arrival_time = At;
        this.Free_seats = fS;
    }

    public string Number{get;set;}
    public string City{get;set;}
    public System.TimeSpan Departure_time{get;set;}
    public int Free_seats { get; set; }
    public System.TimeSpan Arrival_time { get; set; }
    }
}