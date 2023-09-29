8.08:
What are handlers available to do on pipeline?
 - can trace request, if it's logging behavior
 - can mutate request, implementing real pipeline
 - can short-curcuit pipeline
 - can mutate response
 - can trace response
- behaviors
    these can mutate request, when it's being passed down, or not
- handlers
    these are in general - endoints, so do Request -> Response transformation
All these can be generalized with 4 generic params: TRequestIn, TRequestOut, TResponseIn, TResponseOut
And so, there are 4 implementations:
- completely immutable (2 params)
- request mutating (3 params)
- response mutating (3 params)
- mutating both (4 params)

7.08:
What are the possible things on the pipeline?
- behaviors
    these can mutate request, when it's being passed down, or not
- handlers
    these are in general - endoints, so do Request -> Response transformation
All these can be generalized with 4 generic params: TRequestIn, TRequestOut, TResponseIn, TResponseOut
And so, there are 4 implementations:
- completely immutable (2 params)
- request mutating (3 params)
- response mutating (3 params)
- mutating both (4 params)

The most vital question is pipeline resolution (with ordering) and ambiguity detection
- due to generic DI registrations - no problem with resolution of generic handlers
- each handler is registered in DI during discovery
- when route is being built - it should complain if many options available with different result options
- afterwards - route compilations are to be cached



Pipeline A -> C
- Find all request processors
- find all request handlers, that have no mutation: A -> A
- find only one, that does mutation: A -> B. If there would be A -> C. If not only one - MediatorAmbiguityException
- transform: A -> B
- find all request handlers, that have no mutation: B -> B
- transform: B -> C
- find all request handlers, that have no mutation: B -> B