# CMS - A Convention Management System
This is a prototype of a convention management system, which manages convention bookings, relate events and venues.

The system consists of
- Frontend SPA application
- Web API service
- Cosmos DB

## Online deployment
The web application and backend service are deployed in Azure. Click [here](https://happy-sky-0a342490f.1.azurestaticapps.net) to access the applicaiton.
The application is just for demonstration purpose. There might be security issues and bugs. 

Known issue: the authentication flow is sometimes broken after page refresh. It might be related to this [issue](https://github.com/auth0/auth0-spa-js/issues/52).

## Authentication & Authorisation
The system uses [Auth0](https://auth0.com/) as identity provider to manage users. The Frontend uses OpenID Connect (authorisation code flow with PKCE) to authenticate users and uses OAuth2 access token to access backend Web API. 

## Backend service
The backend service provides REST API to manage conventions and bookings. See [Swagger documentation](https://cmsbackend.azurewebsites.net/swagger/index.html) for APIs. 
Role based authorisation access control is applied for API calls:
- Only administrators can create, update or delete conventions
- Only registered users can register for conventions or events.
- Anonymous users can view the conventions and events.

## Frontend
The frontend is a SPA developed in Vue framework.
