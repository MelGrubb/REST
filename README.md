# REST Implementation Patterns

There are plenty of articles out there describing REST as a philosophy, or describing one particular library or another for consuming REST APIs, but I wanted to take some time to talk about implementation patterns I've seen and have used when building REST services using ASP.Net. Before we begin though, let's take at least a few moments to define the basic terminology of REST services.

## What is REST again?

REST stands for "Representational State Transfer". The phrase was coined way back in the year 2000 by Roy Fielding (https://www.ics.uci.edu/~fielding/pubs/dissertation/rest_arch_style.htm), and while the name may not communicate much on its own, the idea is actually pretty simple. It's a way of thinking of and describing data as stateless "resources" that we access using standard HTTP verbs. Each resource has a unique address of its own, and can be manipulated in various ways through this address. The HTTP verbs describe the actions we want to take on these resources, and the server indicates the success or failure of those operations using standard HTTP response codes. There's a lot more to it, of course, but at a high level this is what we're after, a predictable way of organizing and exposing an API so that consumers can know where to find things based on convention.

## CRUD and REST

The basic CRUD (Create, Retrieve, Update, Delete) operations map nicely to the basic HTTP verbs POST, GET, PUT, and DELETE, respectively. We be use the combination of a resource's route and an HTTP verb to define the endpoints that make up our API's supported operations. Rather than posting a payload to some arbitrary endpoint like http://yourservice/accounts/updateperson, we'll define a simple route to the *resource* known as "person", and then implement the appropriate code there. In this example, an update, we'd implement an endpoint for the PUT or PATCH verb, depending on how we intend the caller to use it. Let's look at the verbs you might typically find in a REST API implementation.

### GET

GET retrieves the resource at the given route. REST APIs will commonly support two different GET endpoints for each route, one that takes no additional route parameters and returns a list of resources, and one that takes an identifier as part of the route and returns information about a single resource. Note that I specifically said "route parameter" there. You are perfectly welcome to add parameters to the end of the query string to provide search criteria or to control things like paging, but there will generally be just these two GET routes per resource type. For instance, a call to http://yourservice/person would retrieve a list of people in the system whereas http://yourservice/person/1 would retrieve detailed information about just a single person. Paging parameters could be passed to the first endpoint in the form http://yourservice/person?start=0&length=10, and filtering information might be passed to the second endpoint in the form http://yourservice/person/1?includeAddress=true.

### POST

POST creates a new resource from scratch. This is an insert operation, not an update, so a POST to http://yourservice/person would be expected to create a brand new person object, even if you sent in a person record identical to an existing one in every way, to the extent that you can generally expect any Id value you supply to be ignored by the back-end system.

### PUT

PUT replaces the identified resource. You can think of this as literally "Put this there". It can be thought of as an "insert or update" (upsert) operation, creating new resources or updating existing ones as appropriate. The expectation is that any existing data will be overwritten entirely.

### PATCH 

PATCH makes changes to an existing resource without having to send the entire resource again like you would with PUT. This can be a little more difficult to implement, and takes some planning. Typically, you would expect to send in just the details that you want to update. For instance, if we take a PATCH call to update a person, and that call specified only the person's unique Id and first name, we would be expect it to leave all of the person's other properties, such as last name, alone.

### DELETE

DELETE does exactly what you would expect, deleting the identified resource from the back-end system.

### Less Common Verbs

These are not the only HTTP verbs, and RESTful services may wish to implement others such as OPTIONS and HEAD, but they are outside the scope of this article. Most of the time, you only need the basic four or five verbs above to accomplish everything you need.

## Routes are arbitrary

In ASP.Net, routes follow certain conventions based on the name of the controller class and the name of the method, but these can be overridden by an attribute on the implementing class or method, meaning that you can arrange and expose your routes independently of how the code that implements them is organized. I could handle the route http://yourservice/person in the PersonController, as most developers would expect, or I could do it in a UserController or AccountController class instead. It's completely up to the developer.

This allows for great flexibility, and allows you to give your public API a different "shape" than the underlying data, but it can also lead to some confusion for the developers when it comes time to debug something, as they search the code to find where the problematic endpoint is implemented. For most projects, developers will tend to keep the route and its implementation as similar as possible. If the route ends in "person", they implement its various methods on the PersonController class. 

As a developer, you'll need to put a lot of thought into the decision of how to arrange your code, how to arrange your routes, how similar those two concepts will be, and which one drives the other. When you're adding a new endpoint, how do you decide where it should go? There is no one single answer. Like everything else in the development world, it depends. Let's take a look at two different approaches to this problem.

## Organizing by subject

The most common pattern I've seen is to group API endpoints together by subject, such as putting all the person-related operations in a single PersonController. You then use descriptive method names to promote discoverability and to avoid collisions. For example, an endpoint to get a person's recent orders might be implemented like this.

```csharp
[ApiController]
[Route("person")]
public class PersonController : ControllerBase
{
    [HttpGet("{id}/orders/recent")]
    public ActionResult<List<Order>> GetRecentOrders(int id)
    {
        var results = People.FirstOrDefault(x => x.Id == id)
            .Orders
            .OrderByDescending(x => x.OrderDate)
            .Take(10);

        return results;
    }
}
```

As you can see, the Route attribute has defined the public route for this endpoint (.../person/1/orders/recent), while the method name (GetRecentOrders) is more developer-friendly. An external consumer would issue a GET request to http://yourservice/person/1/orders/recent, and this method would handle that call. Note that the method could have been called simply "RecentOrders", or "Recent", or even just "Get" since there are no other methods on this controller so far. The name of the method does not *have* to match its route in any way.

This is all very simple and easy at first, but following this convention means that everything person-related ends up being implemented in this one controller class. For the higher-traffic areas of your API, this might result in some very large controller classes with dozens of methods, making it hard to navigate, and resulting in merge conflicts as multiple team members regularly find themselves working on the same file at the same time.

You could also make an argument that the "resource" in the example above is actually the order, and the person is merely a query parameter. This is where API design becomes creative. You need to decide how to organize things in a way that makes the most sense to your API's consumers. Does "/orders/recent/{personId}" make more sense? Is it more or less discoverable? Maybe the team decides that the external route is fine, but the method really belongs on the OrderController class since it has more to do with orders than with the person, but they don't want to break existing API consumers. To make this change, we'd just need to move the GetRecentOrders method, fix up the Route attribute, and no-one outside of the development team would even know anything had changed. 

If the team were to decide that the route itself needs to change, what happens to the existing API users then? If we change the route, won't we break the existing users? Fortunately, no. A single endpoint can have multiple routes, so we could keep the old route, as well as establishing a new one, and then gently encourage people to move to the new route when they can. Eventually, when there's no-one left using the old route, we can delete it.

This flexibility is both a strength and a weakness. The ability for the external API to vary independently of its implementation lets us rearrange the code however we want without breaking our existing users, but it also means that developers may have to search a little longer to find what they're looking for because it could be almost anywhere. Even if we're diligent about it, there's still a lot of room for variation within this standard.

### Organizing by route

You could also choose to map the methods onto controllers strictly by their resources, as determined by their routes, and named directly for the HTTP verb that they handle. This is what ASP.Net really *wants* you to do by convention. You can almost do away with the Route attributes completely if you follow the convention closely enough. Out of the box, a method called "Get" or "GetAsync" on the PersonController class will be automatically wired up to handle GET requests to the "/person" route, and that's the key to organizing by route. Here, you can see two controllers, each with a single method called "Get".

```csharp
[ApiController]
[Route("person")]
public class PersonController : ControllerBase
{
    [HttpGet("{id}")]
    public ActionResult<Person> Get(int id)
    {
        return People.FirstOrDefault(x => x.Id == id);
    }
}

[ApiController]
[Route("order")]
public class OrderController : ControllerBase
{
    [HttpGet("{id}")]
    public ActionResult<Order> Get(int id)
    {
        return Orders.FirstOrDefault(x => x.Id == id);
    }
}
```

You can only have a single method with a given signature in any given class. Trying to add a second method with the same name and parameters will result in a compile-time error. The code simply won't build. You can also only have a single method that handles a given route and verb combination or you'll get a run-time error. These two concepts align nicely with each other. The resulting convention forces you to replace small number of very large controllers with a larger number of much smaller controllers, containing only a few methods each, one for each HTTP verb that is supported at that route.

This approach makes the location of the core implementing any given route predictable. Developers will know just where to look to find the implementation of any given route/verb combination. Spreading the code out over more files like this has the beneficial side-effect of reducing merge conflicts with other team members, reducing friction and increasing overall team productivity. Let's look at an example.

If we have a route such as http://yourservice/person/1/order, we would implement that as a method called simply "Get" (or "GetAsync") on a controller called "PersonOrderController". 

```csharp
[ApiController]
public class PersonOrderController : ControllerBase
{
    [HttpGet("person/{personId}/order")]
    public ActionResult<List<Order>> Get(int personId)
    {
        return People.FirstOrDefault(x => x.Id == id).Orders.ToList();
    }
}
```

To come up with this name, we simply concatenate together all of the words in the route that aren't parameters. In this example, that would be "person" and "order" because the "1" is a parameter indicating which person's orders we're supposed to be retrieving. Finally, we tack "Controller" on the end to follow ASP.Net's own controller naming convention, and we're all set.

The distinction between what is part of the route and what is a parameter is sometimes obvious. Other times, it's more of a judgment call. Let's suppose that we want to implement two new endpoints: http://yourservice/person/1/order/recent and http://yourservice/person/1/order/open. First, we need to identify which words identify the resource type, and which are merely parameters. It's obvious that the "1" represents the Id number of the person whose orders we want to retrieve, but what about "recent" and "open"? Are recent orders considered a completely different resource type than open orders? If so, then you could make a strong argument for treating them as part of the route, resulting in a PersonOrderRecentController and a PersonOrderOpenController class, each with a single Get method on them.

```csharp
[ApiController]
public class PersonOrderRecentController : ControllerBase
{
    [HttpGet("person/{personId}/order/recent")]
    public ActionResult<List<Order>> Get(int personId)
    {
        return People.FirstOrDefault(x => x.Id == id).Orders
            .OrderByDescending(x => x.OrderDate)
            .Take(10)
            .ToList();
    }
}

[ApiController]
public class PersonOrderOpenController : ControllerBase
{
    [HttpGet("person/{personId}/order/open")]
    public ActionResult<List<Order>> Get(int personId)
    {
        return People.FirstOrDefault(x => x.Id == id).Orders
            .Where(x => x.Status == "open")
            .ToList();
    }
}
```

Is this a good design though? Is it intuitive? Will the developers find things where they expect? In this case, the answer it "probably not". The names are clunky and something just feels "off" about them. When in doubt, ask yourself the following questions. Do these endpoints accept and return different types? Will either of these controllers have any other methods of their own? If so, then they qualify as being distinct new routes/resources. In this example though, the answer to both question is "no". Neither method takes any kind of parameters that are unique to that endpoint, and both most likely return the same kind of data, perhaps an order summary class. In addition, you would never POST, PUT, or PATCH, or DELETE anything to either of these new routes. Given these answers, it appears that "recent" and "open" do not define a new resource. They are merely parameters to the same GET endpoint (http://yourservice/person/{personId}/orders/{category}).

```csharp
[HttpGet("person/{personId}/order/{category}")]
public ActionResult<List<Order>> Get(int personId, string category)
{
    switch (category)
    {
        case "open":
            return People.FirstOrDefault(x => x.Id == personId).Orders
                .Where(x => x.Status == "open")
                .ToList();
        case "recent":
            return People.FirstOrDefault(x => x.Id == personId).Orders
                .OrderByDescending(x => x.OrderDate)
                .Take(10)
                .ToList();
        default:
            return null;
    }
}
```

## Conclusion

If you want to call your services RESTful, then the HTTP verbs have to be an integral part of the implementation of your routes. It's not enough to simply say that you have REST services simply because you're exposing functionality over HTTP. You need to use the combination of the same route with different verbs to represent different operations on the same piece of data. If you have routes that end in "GetOrders" or "UpdateOrder", then your services might be REST-ish, but they are not truly RESTful. You can specify your routes by hand, expose a RESTful API, and still have a disorganized mess in your controllers. Organizing your controllers and methods according to the routes they handle solves so many development problems that I don't know why you would organize your code any other way. It's a convention that encourages you to keep your code organized, and your classes small. It pushes back when you try to do anything else, and helps you to discover the better solution, and those are the best kinds of conventions.