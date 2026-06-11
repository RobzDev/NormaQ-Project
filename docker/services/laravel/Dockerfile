FROM php:8.3-fpm-alpine

# 1. Instalar dependencias de SISTEMA permanentes (Runtime)
# Estas NO se borran porque las extensiones de PHP las usan para funcionar.
RUN apk add --no-cache \
    libpq \
    libzip \
    libxml2 \
    oniguruma \
    zip \
    unzip \
    git \
    curl
    
RUN apk add --no-cache \
    libreoffice \
    openjdk17-jre-headless \
    fontconfig \
    font-liberation \
    font-noto \
    && fc-cache -fv
# 2. Instalar dependencias de COMPILACIÓN (Build-time)
# Estas sí se borran al final para que la imagen pese menos.
RUN set -eux; \
    apk add --no-cache --virtual .build-deps \
        $PHPIZE_DEPS \
        postgresql-dev \
        libzip-dev \
        libxml2-dev \
        oniguruma-dev; \
    \
    # Instalar extensiones de PHP
    docker-php-ext-install \
        pdo_pgsql \
        bcmath \
        xml \
        mbstring \
        zip \
        opcache; \
    \
    # Instalar Redis vía PECL
    pecl install redis && docker-php-ext-enable redis; \
    \
    # Limpiar dependencias de compilación
    apk del .build-deps

# 3. Composer (Copiamos el binario oficial)
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

WORKDIR /var/www/html

# 4. Manejo de Permisos
# Nota: Como usas volúmenes (bind mounts), el chown aquí solo sirve para 
# carpetas internas. Los permisos de tu PC se sobreescriben al montar el volumen.
RUN mkdir -p /var/www/html/storage /var/www/html/bootstrap/cache \
    && chown -R www-data:www-data /var/www/html \
    && chmod -R 775 /var/www/html/storage /var/www/html/bootstrap/cache

USER www-data