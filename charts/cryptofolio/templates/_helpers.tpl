{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "cryptofolio.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
API name.
*/}}
{{- define "cryptofolio-api.name" -}}
{{- .Values.api.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
API fullname.
*/}}
{{- define "cryptofolio-api.fullname" -}}
{{- if .Values.api.fullnameOverride }}
{{- .Values.api.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := .Values.api.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
API common labels.
*/}}
{{- define "cryptofolio-api.labels" -}}
helm.sh/chart: {{ include "cryptofolio.chart" . }}
{{ include "cryptofolio-api.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
API selector labels.
*/}}
{{- define "cryptofolio-api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "cryptofolio-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the handlers job service account to use.
*/}}
{{- define "cryptofolio-api.serviceAccountName" -}}
{{- if .Values.api.serviceAccount.create }}
{{- default (include "cryptofolio-api.fullname" .) .Values.api.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.api.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Collector job name.
*/}}
{{- define "cryptofolio-collector-job.name" -}}
{{- .Values.jobs.collector.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Collector job fullname.
*/}}
{{- define "cryptofolio-collector-job.fullname" -}}
{{- if .Values.jobs.collector.fullnameOverride }}
{{- .Values.jobs.collector.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := .Values.jobs.collector.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Collector job common labels.
*/}}
{{- define "cryptofolio-collector-job.labels" -}}
helm.sh/chart: {{ include "cryptofolio.chart" . }}
{{ include "cryptofolio-collector-job.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Collector job selector labels.
*/}}
{{- define "cryptofolio-collector-job.selectorLabels" -}}
app.kubernetes.io/name: {{ include "cryptofolio-collector-job.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the collector job service account to use.
*/}}
{{- define "cryptofolio-collector-job.serviceAccountName" -}}
{{- if .Values.jobs.collector.serviceAccount.create }}
{{- default (include "cryptofolio-collector-job.fullname" .) .Values.jobs.collector.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.jobs.collector.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Handlers job name.
*/}}
{{- define "cryptofolio-handlers-job.name" -}}
{{- .Values.jobs.handlers.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Handlers job fullname.
*/}}
{{- define "cryptofolio-handlers-job.fullname" -}}
{{- if .Values.jobs.handlers.fullnameOverride }}
{{- .Values.jobs.handlers.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := .Values.jobs.handlers.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Handlers job common labels.
*/}}
{{- define "cryptofolio-handlers-job.labels" -}}
helm.sh/chart: {{ include "cryptofolio.chart" . }}
{{ include "cryptofolio-handlers-job.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Handlers job selector labels.
*/}}
{{- define "cryptofolio-handlers-job.selectorLabels" -}}
app.kubernetes.io/name: {{ include "cryptofolio-handlers-job.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the handlers job service account to use.
*/}}
{{- define "cryptofolio-handlers-job.serviceAccountName" -}}
{{- if .Values.jobs.handlers.serviceAccount.create }}
{{- default (include "cryptofolio-handlers-job.fullname" .) .Values.jobs.handlers.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.jobs.handlers.serviceAccount.name }}
{{- end }}
{{- end }}
