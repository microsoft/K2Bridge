## Routing

Routing incoming requests to the k2 bridge can be tricky if the "Controller" route is dynamic.

A default Route in asp.net web api is constructed by the convention `{Controller}/{Action}/{param}`.

However, in some cases the incoming requests from Kibana contain dynamic data where the "Controller" name is expected.

For example, a "Field Caps" request may look like:
```
POST /kibana_sample_data_logs/_field_caps?fields=*&ignore_unavailable=true&allow_no_indices=false HTTP/1.1
```
Where `kibana_sample_data_logs` which is the name of the index, looks like a controller, while in fact it is a paramеter.

This is why new routing rules were introduced, which rewrite the path of the request to match the naming convention of the asp.net webapi controllers as follows:
```
/FieldCapability/Process/{indexName}
```
which means in our example it will be routed to `/FieldCapability/Process/kibana_sample_data_logs`


Currently the following Controllers handle the special routing rules:
* FieldCapability
* IndexList

Both follow the same pattern name, and use similar Routing Rules.
