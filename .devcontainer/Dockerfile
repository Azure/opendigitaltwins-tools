# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

FROM mcr.microsoft.com/devcontainers/universal:linux

# Avoid warnings by switching to noninteractive
ENV DEBIAN_FRONTEND=noninteractive

# Set env for tracking that we're running in a devcontainer
ENV DEVCONTAINER=true

# Save command line history
RUN echo "export HISTFILE=/home/codespace/commandhistory/.bash_history" >> "/home/codespace/.bashrc" \
    && echo "export PROMPT_COMMAND='history -a'" >> "/home/codespace/.bashrc" \
    && mkdir -p /home/codespace/commandhistory \
    && touch /home/codespace/commandhistory/.bash_history \
    && chown -R codespace:codespace /home/codespace/commandhistory

# Git command prompt
RUN git clone https://github.com/magicmonty/bash-git-prompt.git /home/codespace/.bash-git-prompt --depth=1 \
    && echo "if [ -f \"/home/codespace/.bash-git-prompt/gitprompt.sh\" ]; then GIT_PROMPT_ONLY_IN_REPO=1 && source /home/codespace/.bash-git-prompt/gitprompt.sh; fi" >> "/home/codespace/.bashrc" \
    && chown -R codespace:codespace /home/codespace/.bash-git-prompt

# terraform + tflint
ARG TERRAFORM_VERSION=1.11.2
ARG TFLINT_VERSION=0.55.1
RUN mkdir -p /tmp/docker-downloads \
    && curl -sSL -o /tmp/docker-downloads/terraform.zip https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION}/terraform_${TERRAFORM_VERSION}_linux_amd64.zip \
    && unzip /tmp/docker-downloads/terraform.zip \
    && mv terraform /usr/local/bin \
    && rm /tmp/docker-downloads/terraform.zip \
    && echo "alias tf=terraform" >> "/home/codespace/.bashrc"

RUN curl -sSL -o /tmp/docker-downloads/tflint.zip https://github.com/wata727/tflint/releases/download/v${TFLINT_VERSION}/tflint_linux_amd64.zip \
    && unzip /tmp/docker-downloads/tflint.zip \
    && mv tflint /usr/local/bin \
    && rm /tmp/docker-downloads/tflint.zip

# azure-cli
COPY ./scripts/azure-cli.sh /tmp/
RUN /tmp/azure-cli.sh

# Install PowerShell
RUN sudo apt-get update \
    && sudo apt-get install -y wget apt-transport-https software-properties-common \
    && wget -q "https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb" -O packages-microsoft-prod.deb \
    && sudo dpkg -i packages-microsoft-prod.deb \
    && sudo apt-get update \
    && sudo apt-get install -y powershell \
    && sudo apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# azure developer cli (azd)
RUN curl -fsSL https://aka.ms/install-azd.sh | bash

# Sync timezone (if TZ value not already present on host it defaults to America/Los_Angeles)
# Note: if running on WSL (Windows) you can add the below to your $profile so your tz is automatically synced
# $tz =  [Windows.Globalization.Calendar,Windows.Globalization,ContentType=WindowsRuntime]::New().GetTimeZone()
# [Environment]::SetEnvironmentVariable("TZ",$tz, "User")
RUN if [ -z "$TZ" ]; then TZ="America/Los_Angeles"; fi && sudo ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ | sudo tee /etc/timezone > /dev/null

# Install gettext-base so that we have envsubst
RUN sudo apt-get update \
    && sudo apt-get -y install gettext-base

# Switch to the built-in non-root user
USER codespace