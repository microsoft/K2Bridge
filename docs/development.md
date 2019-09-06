## Development

### Setting up local environment
1. Download [Kibana](https://www.elastic.co/downloads/past-releases/kibana-6-8-1) (currently we support version 6.8) and unpack
2. Under Kibana's '*config*' directory, edit the *kibana.yml* file and make sure the following lines exists:
    - elasticsearch.hosts: ["http://127.0.0.1:8080"] # This should point to your local HTTP sniffer (like fiddler) or directly to your local bridge
	- elasticsearch.customHeaders: {x-fiddler-reroute: 1} # When using fiddler in proxy mode this helps avoid unwanted reroutes of the requests. It won't harm to have it always.
3. Clone KibanaKustoBridge repository
4. Update the values in the [appsettings.json](../K2Bridge/appsettings.json) file
    - assign the proxy url to localhost:
    ```
    "bridgeListenerAddress": "http://127.0.0.1:8080/",
    ```
5. Start the proxy
6. Browse to *http://localhost:5601/* to start the Kibana UI

#### Testing
1. Go to the *Discover* tab
2. Choose a time window which includes data
3. Search
4. Entries from the Kusto table should appear in the search result

![Example](./images/search_example.png)