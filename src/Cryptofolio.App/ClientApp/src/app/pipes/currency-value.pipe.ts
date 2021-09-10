import { Inject, LOCALE_ID, Pipe, PipeTransform } from "@angular/core";
import { formatNumber } from "@angular/common";
import { Currency } from "../models/currency";

@Pipe({name: 'currencyValue'})
export class CurrencyValuePipe implements PipeTransform {
  constructor(@Inject(LOCALE_ID) private locale: string) {}

  transform(value: number, currency: Currency) {
    return currency.valueFormat
      .replace("{0}", formatNumber(value, this.locale, "1.0-2"))
      .replace("{1}", currency.symbol);
  }
}