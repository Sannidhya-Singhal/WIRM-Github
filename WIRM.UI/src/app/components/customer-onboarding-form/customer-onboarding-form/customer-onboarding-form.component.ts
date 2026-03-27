import { AfterViewInit, Component, ElementRef, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormArray, FormControl, FormGroup, FormsModule, NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  createCustomerOnboardingForm,
  createCustomizedTemplateRow,
  createModifierRow,
  createOtherUserRow,
  CustomizedTemplateRowForm,
  ModifierRowForm,
  OtherUserRowForm,
} from '../../../shared/types/customer-onboarding-form.type';
import { AsyncPipe } from '@angular/common';
import { AutoExpandTextAreaDirective } from '../../../shared/directives/auto-expand-text-area.directive';
import { KebabToWordsPipe } from '../../../shared/pipes/kebab-case-to-words.pipe';
import { BehaviorSubject } from 'rxjs';
import { CustomerOnboardingService } from '../../../services/customer-onboarding-service';

@Component({
  selector: 'app-customer-onboarding-form',
  standalone: true,
  templateUrl: './customer-onboarding-form.component.html',
  styleUrl: './customer-onboarding-form.component.css',
  imports: [
    AsyncPipe,
    FormsModule,
    ReactiveFormsModule,
    KebabToWordsPipe,
    AutoExpandTextAreaDirective,
  ]
})
export class CustomerOnboardingFormComponent implements OnInit,AfterViewInit {
   @ViewChild('inputStep1') inputStep1!: ElementRef<HTMLInputElement>;
   @ViewChild('inputStep2') inputStep2!: ElementRef<HTMLInputElement>;
   @ViewChild('inputStep3') inputStep3!: ElementRef<HTMLInputElement>;
   @ViewChild('inputStep4') inputStep4!: ElementRef<HTMLInputElement>;
  @ViewChild('inputStep5') inputStep5!: ElementRef<HTMLInputElement>;
   @ViewChild('inputStep6') inputStep6!: ElementRef<HTMLInputElement>;
  formBuilder = inject(NonNullableFormBuilder);
  private readonly customerOnboardingService = inject(CustomerOnboardingService);
  customerOnboardingForm = createCustomerOnboardingForm(this.formBuilder);

  submitFormMessage$ = new BehaviorSubject<string>('');
  submitFormErrorMessage$ = new BehaviorSubject<string>('');
  isSubmittingForm$ = new BehaviorSubject<boolean>(false);

  submitted = false;
  isFormSubmitted = false;

  /** Earliest selectable day for required deployment date (today, local). */
  minDeploymentDate = '';

  readonly maxOtherUsers = 5;
  readonly maxCustomizedTemplateRows = 5;
  readonly maxModifierRows = 3;

  /** Predefined options for Process Specifics multi-select fields */
  readonly availableAddOnOptions = ['DTP','DTP + LSO','Online Review'];
  readonly workTypeOptions = ['New Setup', 'Migration', 'Enhancement', 'Support', 'Maintenance'];
  readonly serviceOptions = ['Consulting', 'Implementation', 'Training', 'Managed Service', 'Custom Development'];

  /** Track which multi-select dropdown is open: e.g. 'addOns-0', 'workType-1', 'service-2' */
  openMultiSelectKey: string | null = null;

  /** Single optional Excel upload for Other Users (Step 3). */
  otherUsersExcelFile: File | null = null;
  otherUsersUploadMessages: string[] = [];
  readonly maxOtherUsersExcelMb = 60;
  readonly maxOtherUsersExcelBytes = this.maxOtherUsersExcelMb * 1024 * 1024;
  readonly allowedOtherUsersExcelExtensions = ['xlsx', 'xls', 'xlsm'];

  get otherUsers(): FormArray<FormGroup<OtherUserRowForm>> {
    return this.customerOnboardingForm.controls.otherUsers;
  }

  get customizedTemplates(): FormArray<FormGroup<CustomizedTemplateRowForm>> {
    return this.customerOnboardingForm.controls.customizedTemplates;
  }

  get businessModifiers(): FormArray<FormGroup<ModifierRowForm>> {
    return this.customerOnboardingForm.controls.businessModifiers;
  }

  get financeModifiers(): FormArray<FormGroup<ModifierRowForm>> {
    return this.customerOnboardingForm.controls.financeModifiers;
  }

  get processModifiers(): FormArray<FormGroup<ModifierRowForm>> {
    return this.customerOnboardingForm.controls.processModifiers;
  }

  ngAfterViewInit(): void {}

  ngOnInit(): void {
    const d = new Date();
    this.minDeploymentDate = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    // Add one default row for each dynamic section
    this.addOtherUserRow();
    this.addCustomizedTemplateRow();
    this.addModifierRow(this.businessModifiers);
    this.addModifierRow(this.financeModifiers);
    this.addModifierRow(this.processModifiers);
  }

  addOtherUserRow(): void {
    if (this.otherUsers.length >= this.maxOtherUsers) {
      return;
    }
    this.otherUsers.push(createOtherUserRow(this.formBuilder));
  }

  removeOtherUserRow(index: number): void {
    this.otherUsers.removeAt(index);
  }

  onOtherUsersExcelSelected(event: Event): void {
    this.otherUsersUploadMessages = [];
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (!files?.length) {
      input.value = '';
      return;
    }
    const file = files[0];
    const ext = this.getUploadFileExtension(file);
    if (!ext || !this.allowedOtherUsersExcelExtensions.includes(ext)) {
      this.otherUsersUploadMessages.push(
        'File type not supported. Please upload an Excel file (.xlsx, .xls, .xlsm).',
      );
      input.value = '';
      return;
    }
    if (file.size > this.maxOtherUsersExcelBytes) {
      this.otherUsersUploadMessages.push(
        `File ${file.name} (${this.getUploadFileSizeMb(file)} MB) exceeds ${this.maxOtherUsersExcelMb} MB limit.`,
      );
      input.value = '';
      return;
    }
    this.otherUsersExcelFile = file;
    input.value = '';
  }

  removeOtherUsersExcel(): void {
    this.otherUsersExcelFile = null;
    this.otherUsersUploadMessages = [];
  }

  getUploadFileExtension(file: File): string | undefined {
    if (!file.name.includes('.')) return undefined;
    return file.name.split('.').pop()?.toLowerCase();
  }

  getUploadFileSizeMb(file: File): number {
    return +(file.size / (1024 * 1024)).toFixed(2);
  }

  addCustomizedTemplateRow(): void {
    if (this.customizedTemplates.length >= this.maxCustomizedTemplateRows) {
      return;
    }
    this.customizedTemplates.push(createCustomizedTemplateRow(this.formBuilder));
  }

  removeCustomizedTemplateRow(index: number): void {
    this.customizedTemplates.removeAt(index);
  }

  /** Toggle a multi-select dropdown panel open/closed. */
  toggleMultiSelect(key: string): void {
    this.openMultiSelectKey = this.openMultiSelectKey === key ? null : key;
  }

  /** Close multi-select dropdown when clicking outside of it. */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.openMultiSelectKey) return;
    const target = event.target as HTMLElement;
    if (!target.closest('.multi-select-toggle') && !target.closest('.multi-select-panel')) {
      this.openMultiSelectKey = null;
    }
  }

  /** Toggle a single option within a FormControl<string[]>. */
  toggleOption(control: FormControl<string[]>, option: string): void {
    const current = control.value;
    if (current.includes(option)) {
      control.setValue(current.filter(v => v !== option));
    } else {
      control.setValue([...current, option]);
    }
  }

  /** Check if an option is selected in a FormControl<string[]>. */
  isSelected(control: FormControl<string[]>, option: string): boolean {
    return control.value.includes(option);
  }

  addModifierRow(list: FormArray<FormGroup<ModifierRowForm>>): void {
    if (list.length >= this.maxModifierRows) {
      return;
    }
    list.push(createModifierRow(this.formBuilder));
  }

  removeModifierRow(list: FormArray<FormGroup<ModifierRowForm>>, index: number): void {
    list.removeAt(index);
  }

  onEnterPress(event: KeyboardEvent | Event) {
    const target = event.target as HTMLElement;
    if (target.tagName.toLowerCase() !== 'textarea') {
      event.preventDefault();
    }
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.customerOnboardingForm.invalid) {
      this.customerOnboardingForm.markAllAsTouched();
    }

    this.submitFormErrorMessage$.next('');
    this.submitFormMessage$.next('');

    const c = this.customerOnboardingForm.controls;
    const stepValidations: { step: ElementRef<HTMLInputElement>; controls: AbstractControl[] }[] = [
      { step: this.inputStep1, controls: [c.customerGroupName, c.customerAccountName, c.codaCode] },
      { step: this.inputStep2, controls: [] },
      { step: this.inputStep3, controls: [c.primaryUserLastName, c.primaryUserEmailAddress] },
      { step: this.inputStep4, controls: [] },
      { step: this.inputStep5, controls: [] },
      {
        step: this.inputStep6,
        controls: [
          c.considerUrgentDeployment,
          c.considerOperationalOwnerAccount,
          c.considerAccountManager,
          c.considerMigrationPoc,
        ],
      },
    ];

    if (this.customerOnboardingForm.valid) {
      this.isSubmittingForm$.next(true);

      const formValue: Record<string, unknown> = { ...this.customerOnboardingForm.getRawValue() };

      const isModifierEmpty = (row: Record<string, string>) =>
        !row['name']?.trim() && !row['values']?.trim() && !row['detailsPurpose']?.trim() && !row['expectedBehaviorWhenSelected']?.trim();

      const allModifiersEmpty = (rows: Record<string, string>[]) =>
        !rows?.length || rows.every(isModifierEmpty);

      const isOtherUserEmpty = (row: Record<string, string>) =>
        !row['firstName']?.trim() && !row['lastName']?.trim() && !row['email']?.trim() && !row['role']?.trim();

      const allOtherUsersEmpty = (rows: Record<string, string>[]) =>
        !rows?.length || rows.every(isOtherUserEmpty);

      if (allOtherUsersEmpty(formValue['otherUsers'] as Record<string, string>[])) {
        delete formValue['otherUsers'];
      }
      if (allModifiersEmpty(formValue['businessModifiers'] as Record<string, string>[])) {
        delete formValue['businessModifiers'];
      }
      if (allModifiersEmpty(formValue['financeModifiers'] as Record<string, string>[])) {
        delete formValue['financeModifiers'];
      }
      if (allModifiersEmpty(formValue['processModifiers'] as Record<string, string>[])) {
        delete formValue['processModifiers'];
      }

      const payload = new FormData();
      payload.append('form', JSON.stringify(formValue));
      if (this.otherUsersExcelFile) {
        payload.append('otherUsersExcel', this.otherUsersExcelFile, this.otherUsersExcelFile.name);
      }

      this.customerOnboardingService.submitCustomerOnboarding(payload).subscribe({
        next: (res) => {
          this.isSubmittingForm$.next(false);
          this.isFormSubmitted = true;
          const base =
            res.message?.trim() ||
            'Your customer onboarding request has been submitted successfully.';
          if (res.id != null && res.id !== '') {
          this.submitFormMessage$.next(
  `${base} <a href="${res.link}" target="_blank">View Work Item</a>`
);
          } else {
            this.submitFormMessage$.next(base);
          }
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmittingForm$.next(false);
          let msg = 'Submission failed. Please try again.';
          if (typeof err.error === 'string' && err.error.trim()) {
            msg = err.error;
          } else if (
            err.error &&
            typeof err.error === 'object' &&
            'message' in err.error &&
            String((err.error as { message: unknown }).message).trim()
          ) {
            msg = String((err.error as { message: string }).message);
          } else if (err.status === 0) {
            msg = 'Unable to reach the server. Check your connection and API URL.';
          }
          this.submitFormErrorMessage$.next(msg);
        },
      });
    } else {
      for (const group of stepValidations) {
        const hasError = group.controls.some((ctrl) => ctrl.invalid);
        if (hasError) {
          group.step.nativeElement.checked = true;
          break;
        }
      }
    }
  }

  onReset(): void {
    this.customerOnboardingForm.reset();
    this.otherUsers.clear();
    this.customizedTemplates.clear();
    this.businessModifiers.clear();
    this.financeModifiers.clear();
    this.processModifiers.clear();

    this.addOtherUserRow();
    this.addCustomizedTemplateRow();
    this.addModifierRow(this.businessModifiers);
    this.addModifierRow(this.financeModifiers);
    this.addModifierRow(this.processModifiers);

    this.otherUsersExcelFile = null;
    this.otherUsersUploadMessages = [];

    this.submitted = false;
    this.isFormSubmitted = false;
    this.submitFormMessage$.next('');
    this.submitFormErrorMessage$.next('');

    this.inputStep1.nativeElement.checked = true;
  }
}


