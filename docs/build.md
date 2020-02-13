## Build the Docker container

Define arguments:

```sh
IMAGE_NAME=<IMAGE_NAME e.g. k2bridge>
IMAGE_TAG=<TAG>
IMAGE_WITH_TAG=$IMAGE_NAME:$IMAGE_TAG
```

Build the container using a local Docker installation:

```sh
docker build -t $IMAGE_WITH_TAG .
```
