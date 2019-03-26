# Execution model
When does a node execute?
>A node executes when all its inputs are available

What does the execution loop look like?
>The execution unit keeps a list of nodes with all inputs ready

>It takes the first node with all inputs ready, removes it from the list and executes it, and the execution results is a pair (output, value) that indicates which outputs produced which value.

>Each produced value is propagated to the input of nodes connected to the specified output: each node that becomes ready with this new input is added to the list.

What stops the execution?
>The execution might never stop, because some event nodes might produce outputs without having an input. Or there can be a cycle in the graph. For simpler graphs the execution stop when no node is ready.

Is this model suitable for low level code and tight loops?
>You can do that, but the overhead could be high. The power of this system lies in its graph connections and graphical representation, which is suitable for high level concepts.

What kind of nodes are there?
>Nodes do not have a kind, but are characterized by the type of ports they have.

>A node with 0 inputs and 1+ outputs is an *event source*

>A node with 1+ inputs and 0 outputs is a *data store*. 

>A node with 1+ input and 1+ outputs is a *data transform*

# Pull-type outputs
A data store that writes to disk can in fact have 0 outputs to read it back and still make sense, but an in-memory data store (variables) need a way to be read. 

For this reason, *pull-type outputs* are introduced.

pull-type outputs can be connected to inputs, like regular outputs, but they do not produce values when their node is executed: they implement a lazy evaluation model. When the node of an input they are connected to is activable with respect to its other inputs connected to regular outputs, the system requests the outputs from the remaining pull-type outputs before activating it.