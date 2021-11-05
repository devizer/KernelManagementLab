ARG BASE_IMAGE 
FROM ${BASE_IMAGE}

ENV DEBIAN_FRONTEND=noninteractive \
 ASPNETCORE_URLS="http://0.0.0.0:80" \
 RESPONSE_COMPRESSION=True \
 INSTALL_DIR=/opt/w3top \
 DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
 DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=1 \
 container=docker \
 __LC_ALL=en_US.UTF8 \
 __LANG=en_US.UTF8 \
 BASE_DOCKER_IMAGE=${BASE_IMAGE} \
 BUILD_URL=${BUILD_URL} \
 JOB_URL=${JOB_URL} \
 BUILD_SOURCEVERSION=${BUILD_SOURCEVERSION} \
 BUILD_SOURCEBRANCHNAME=${BUILD_SOURCEBRANCHNAME} \
 BUILD_BUILDID=${BUILD_BUILDID}

EXPOSE 80/tcp

WORKDIR /opt/w3top
SHELL ["/bin/bash", "-c"]

RUN echo; \
 export HTTP_PORT RESPONSE_COMPRESSION INSTALL_DIR DOTNET_SYSTEM_GLOBALIZATION_INVARIANT DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER; \
 apt-get update; apt-get install sudo curl libstdc++6 -y -qq; \
 # url=https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-dependencies.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -ksSL $url) | UPDATE_REPOS=true bash -e && echo "Successfully installed .NET Core Dependencies"; \
 script=https://raw.githubusercontent.com/devizer/w3top-bin/master/install-w3top-service.sh; \
 (curl -ksSL $script) | bash; \
 apt-get purge curl libunwind8 -y -q; \
 rm -rf /var/cache/apt/*; rm -rf /tmp/*; rm -rf /var/lib/apt/*; \
 echo "COMPLETED";
 
CMD ["/opt/w3top/Universe.W3Top"]


