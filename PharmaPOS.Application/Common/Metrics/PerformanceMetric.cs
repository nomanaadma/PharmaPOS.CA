using System.Diagnostics.Metrics;

namespace PharmaPOS.Application.Common.Metrics;

public class PerformanceMetric
{
	public Histogram<double> RequestDuration { get; }

	public PerformanceMetric(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("PharmaPOS.Application");
        
		// Histograms are the correct instrument for measuring latency/duration
		RequestDuration = meter.CreateHistogram<double>(
			"pharmapos.mediatr.request.duration",
			unit: "ms",
			description: "Measures the duration of MediatR requests in milliseconds.");
	}
}