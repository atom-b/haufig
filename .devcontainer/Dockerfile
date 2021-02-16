FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:dev-5.0

# [Option] Install Node.js
ARG INSTALL_NODE="true"
ARG NODE_VERSION="lts/*"
RUN if [ "${INSTALL_NODE}" = "true" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# [Option] Install Azure CLI
ARG INSTALL_AZURE_CLI="false"
COPY library-scripts/azcli-debian.sh /tmp/library-scripts/
RUN if [ "$INSTALL_AZURE_CLI" = "true" ]; then bash /tmp/library-scripts/azcli-debian.sh; fi \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/* /tmp/library-scripts

# Copy endpoint specific user settings into container to specify
# .NET Core should be used as the runtime.
COPY settings.vscode.json /root/.vscode-remote/data/Machine/settings.json

# libpython doesn't add itself to ld.so.conf so use env var instead
ENV LD_LIBRARY_PATH=/lib:/usr/lib:/usr/local/lib:/usr/lib/python3.8/config-3.8-x86_64-linux-gnu/
ARG SPACY_PIPELINE="de_core_news_sm"
# ARG SPACY_PIPELINE="de_dep_news_trf"

# TODO: do we need python3.8 AND libpython?
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends procps python3.8 libpython3.8 python3-wheel python3-pip libc6-dev

RUN pip3 install setuptools && \
    pip3 install spacy spacy[transformer] && \
    python3 -m spacy download ${SPACY_PIPELINE}

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1
