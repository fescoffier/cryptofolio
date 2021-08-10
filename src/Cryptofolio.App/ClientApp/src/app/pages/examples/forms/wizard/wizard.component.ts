import { Component, OnInit } from "@angular/core";
import Stepper from "bs-stepper";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";

@Component({
  selector: "app-wizard",
  templateUrl: "wizard.component.html"
})
export class WizardComponent implements OnInit {
  value = 15;
  multiselect: any = [];
  private stepper: Stepper;
  checked = false;
  checked1 = false;
  checked2 = false;

  focus;
  focus1;
  focus2;
  focus3;
  focus4;

  focusTouched;
  focus1Touched;
  focus2Touched;
  focus3Touched;

  public formWizard: FormGroup;
  wizard = false;
  step = 1;
  constructor(public formBuilder: FormBuilder) {}

  ngOnInit() {
    var wizard = document.getElementsByClassName("card-wizard")[0];
    wizard.classList.add("active");
    var stepper = document.getElementById("wizardProfile");

    this.stepper = new Stepper(stepper, {
      linear: false,
      animation: true
    });

    this.formWizard = this.formBuilder.group({
      firstName: ["", [Validators.required, Validators.minLength(1)]],
      lastName: ["", [Validators.required, Validators.minLength(1)]],
      email: [
        "",
        [Validators.required, Validators.email, Validators.minLength(1)]
      ],
      phone: ["", [Validators.required]],
      select: [""]
    });
  }
  get registerF() {
    return this.formWizard.controls;
  }

  onRegister() {
    this.wizard = true;
    // stop here if form is invalid
    if (this.formWizard.invalid) {
      return;
    }
    this.stepper.next();
  }
  next() {
    if (this.formWizard.valid) {
      console.log("aici");
      if (this.value < 51) {
        this.step++;
        this.value += 35;
      }
      if (this.step === 1) {
        this.checked = true;
      } else if (this.step === 2) {
        this.checked1 = true;
      } else {
        this.checked2 = true;
      }
      this.stepper.next();
    }
  }
  previous() {
    this.stepper.previous();
    if (this.value > 15) {
      this.value -= 35;
      this.step--;
    }
  }
  addCheched(event) {
    event.target.classList.add("checked");
  }
}
