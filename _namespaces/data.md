---
name: Data
title: Data Namespace
layout: default
---

# {{ page.title }}

The Data Namespace contains classes and interfaces for storing mass spectra information.


### Classes
{% for p in site.ns_data %}
{% if p.class == 'class' %}
<a href="{{ "/" | absolute_url }}{{ p.url }}"> {{ p.title }}</a> - {{ p.description }}<br/>  
{% endif %}
{% endfor %}


### Structs
{% for p in site.ns_data %}
{% if p.class == 'struct' %}
<a href="{{ "/" | absolute_url }}{{ p.url }}"> {{ p.title }}</a> - {{ p.description }}<br/>  
{% endif %}
{% endfor %}

### Enums
{% for p in site.ns_data %}
{% if p.class == 'enum' %}
<a href="{{ "/" | absolute_url }}{{ p.url }}"> {{ p.title }}</a> - {{ p.description }}<br/>  
{% endif %}
{% endfor %}

### Interfaces
{% for p in site.ns_data %}
{% if p.class == 'interface' %}
<a href="{{ "/" | absolute_url }}{{ p.url }}"> {{ p.title }}</a> - {{ p.description }}<br/>  
{% endif %}
{% endfor %}