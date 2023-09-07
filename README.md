# Akka.NET Application Management Best Practices

This is my code sample for September 7th's [Akka.NET Application Management Best Practices](https://attendee.gotowebinar.com/register/6615024124898419803?source=Twitter) webinar. 

Demonstrates two totally distinct approaches to building the same application:

* A "Clean Code"-inspired [Bespoke Company Framework](https://aaronstannard.com/dry-gone-bad-bespoke-company-framework/): [`Akka.BCF`](https://github.com/Aaronontheweb/akkadotnet-app-management-presentation/tree/master/src/Akka.BCF)
* My preferred and recommended approach - a pattern-driven approach to domain / infrastructure design that doesn't have a framework or many shared abstractions of any kind: [`Akka.Pattern`](https://github.com/Aaronontheweb/akkadotnet-app-management-presentation/tree/master/src/Akka.Pattern)

Made using `dotnet new` and the `Akka.Templates` package. See https://github.com/akkadotnet/akkadotnet-templates/blob/dev/docs/ConsoleTemplate.md for complete and current documentation on this template.
