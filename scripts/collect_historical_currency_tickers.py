#!/usr/bin/env python
#

import argparse
import datetime
import logging
import json
import psycopg2
import requests

FIXER_API = "https://data.fixer.io/api/timeseries"


def fetch_tickers(args):
    logging.info("Calling Fixer timeseries API using the following parameters:\n\taccess_key: ****%s\n\tbase: %s\n\tsymbols: %s\n\tyears: %s",
                 args.access_key[-4:],
                 args.base,
                 args.symbols,
                 args.years)

    tickers = []

    for year in args.years.split(","):
        start_date = f"{year}-01-01"
        end_date = ""
        if year == datetime.date.today().year:
            end_date = f"{year}-{datetime.date.today().month}-{datetime.date.today().day}"
        else:
            end_date = f"{year}-12-31"
        logging.info("Fetching data for the year %s with start_date=%s and end_date=%s.",
                     year,
                     start_date,
                     end_date)
        result = requests.get(
            f"{FIXER_API}?access_key={args.access_key}&base={args.base}&symbols={args.symbols}&start_date={start_date}&end_date={end_date}")
        logging.debug("Response status code: %s", result.status_code)
        logging.debug("Data: \n%s", result.json())

        response = json.loads(result.content)
        tickers.extend([{
            "currency_code": response["base"],
            "vs_currency_code": symbol,
            "timestamp": date,
            "value": response["rates"][date][symbol]
        } for date in response["rates"] for symbol in response["rates"][date]])

    logging.info("Fetched %d tickers from fixer.", len(tickers))
    return tickers



def main(args, loglevel):
    logging.basicConfig(format="%(levelname)s: %(message)s", level=loglevel)

    logging.info("Collecting currency tickers from the Fixer API.")

    tickers = fetch_tickers(args)
    commands = [f"insert into \"data\".\"currency_ticker\" (\"timestamp\",\"currency_id\",\"vs_currency_id\",\"value\") \
                values ('{t['timestamp']}',(select id from \"data\".\"currency\" where \"code\" = '{t['currency_code'].lower()}'),(select id from \"data\".\"currency\" where \"code\" = '{t['vs_currency_code'].lower()}'),{t['value']}) \
                on conflict (\"timestamp\",\"currency_id\",\"vs_currency_id\") do update set \"value\" = {t['value']};"
                for t in tickers]
    logging.info("Opening the database connection.")
    with psycopg2.connect(database=args.database, user=args.user, password=args.password, host=args.host, port=args.port) as con:
      logging.info("Database connection opened.")
      with con.cursor() as cur:
        logging.info("Executing SQL commands.")
        for command in commands:
            logging.debug("Executing SQL command: %s", command)
            cur.execute(command)
        logging.info("Committing to database.")
        con.commit()
        logging.info("Committed to database.")

    logging.info("Currency tickers collected from the Fixer API.")


# Standard boilerplate to call the main() function to begin
# the program.
if __name__ == '__main__':
    parser = argparse.ArgumentParser(
        description="Collects historical currency ticker from the Fixer API.",
        epilog="As an alternative to the commandline, params can be placed in a file, one per line, and specified on the commandline like '%(prog)s @params.conf'.",
        fromfile_prefix_chars='@')
    parser.add_argument(
        "-k",
        "--access_key",
        help="the Fixer access_key",
        metavar="KEY")
    parser.add_argument(
        "-b",
        "--base",
        help="the base symbol",
        metavar="USD")
    parser.add_argument(
        "-s",
        "--symbols",
        help="the versus symbols",
        metavar="EUR,JPY,GBP,CHF,CAD")
    parser.add_argument(
        "-y",
        "--years",
        help="the years",
        metavar=datetime.date.today().year)
    parser.add_argument(
        "-ht",
        "--host",
        help="the database server host",
        metavar="localhost")
    parser.add_argument(
        "-pt",
        "--port",
        help="the database server port",
        metavar="5432")
    parser.add_argument(
        "-d",
        "--database",
        help="the database",
        metavar="cryptofolio")
    parser.add_argument(
        "-u",
        "--user",
        help="the database user",
        metavar="cryptofolio")
    parser.add_argument(
        "-p",
        "--password",
        help="the database password",
        metavar="Pass@word1")
    parser.add_argument(
        "-v",
        "--verbose",
        help="increase output verbosity",
        action="store_true")
    args = parser.parse_args()

    # Setup logging
    if args.verbose:
        loglevel = logging.DEBUG
    else:
        loglevel = logging.INFO

    main(args, loglevel)
