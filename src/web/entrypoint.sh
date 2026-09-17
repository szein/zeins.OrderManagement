#!/bin/sh

sed -i "s|\${API_BASE_URL}|$API_BASE_URL|g" /usr/share/nginx/html/appsettings.json
exec nginx -g 'daemon off;'