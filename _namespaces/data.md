---
name: Data
title: Data Namespace
layout: default
---

### Classes
{% if page.name == 'Data' %}
Success
{% else %}
Fail
{% endif %}


### Structs
{% for page in site.ns_data %}
{% if page.class == 'struct' %}
<a href="/Nova{{ page.url }}"> {{ page.title }}</a><br/>  
{% endif %}
{% endfor %}

### Enums


### Interfaces
