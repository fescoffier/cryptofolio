import { NgModule } from "@angular/core";
import { CurrencyValuePipe } from "./currency-value.pipe";

@NgModule({
  declarations: [
    CurrencyValuePipe
  ],
  exports: [
    CurrencyValuePipe
  ]
})
export class PipeModule { }