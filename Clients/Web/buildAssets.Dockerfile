FROM python
RUN mkdir /public
RUN mkdir /public/assets
RUN mkdir /extracted
COPY ./create-assets.sh .
COPY ./create_assets.py .
ENTRYPOINT [ "/create-assets.sh" ]