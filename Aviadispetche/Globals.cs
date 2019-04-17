namespace Aviadispetcher
{
    public class Flight
    { 
        public Flight(string nF, string cF, System.TimeSpan tF, int fS)
    {
        this.Number = nF;
        this.City = cF;
        this.Departure_time = tF;
        this.Free_seats = fS;
    }

    public string Number{get;set;}
    public string City{get;set;}
    public System.TimeSpan Departure_time{get;set;}
    public int Free_seats { get; set; }
    }
}