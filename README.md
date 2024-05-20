# URL Shortener Service

### This service supports creating a short url for a given long url along with capturing some basic statistics on short url clicks
- Solution hosts a REST endpoint exposing all the service features
- Current solution does not support any persistent storage such as persistent DB/cache/files etc.
- Current solution also does not support any rate limiting measures for the sake of simplicity
- Base 62 encoding (0-9,a-z,A-Z) is used to generate a unique fixed length (configurable) string which is used for shortening the input url
