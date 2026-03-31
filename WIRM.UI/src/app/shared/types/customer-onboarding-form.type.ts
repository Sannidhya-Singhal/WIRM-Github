import { AbstractControl, FormArray, FormControl, FormGroup, NonNullableFormBuilder, ValidationErrors, Validators } from "@angular/forms";

/** Validates that a comma-separated string has at most `max` entries. */
export function maxCommaSeparatedEntries(max: number) {
    return (control: AbstractControl): ValidationErrors | null => {
        const value = (control.value ?? '').trim();
        if (!value) return null;
        const count = value.split(',').filter((entry: string) => entry.trim() !== '').length;
        return count > max ? { maxCommaSeparatedEntries: { max, actual: count } } : null;
    };
}

export type OtherUserRowForm = {
    firstName: FormControl<string>;
    lastName: FormControl<string>;
    email: FormControl<string>;
    preferredLanguage: FormControl<string>;
    role: FormControl<string>;
};

export function createOtherUserRow(fb: NonNullableFormBuilder): FormGroup<OtherUserRowForm> {
    return fb.group<OtherUserRowForm>({
        firstName: fb.control(''),
        lastName: fb.control(''),
        email: fb.control(''),
        preferredLanguage: fb.control(''),
        role: fb.control(''),
    });
}

export type CustomizedTemplateRowForm = {
    desiredName: FormControl<string>;
    existingAuroraProcess: FormControl<string>;
    availableAddOns: FormControl<string[]>;
    workType: FormControl<string[]>;
    service: FormControl<string[]>;
};

export function createCustomizedTemplateRow(fb: NonNullableFormBuilder): FormGroup<CustomizedTemplateRowForm> {
    return fb.group<CustomizedTemplateRowForm>({
        desiredName: fb.control(''),
        existingAuroraProcess: fb.control('No'),
        availableAddOns: fb.control<string[]>([]),
        workType: fb.control<string[]>([]),
        service: fb.control<string[]>([]),
    });
}

export type ModifierRowForm = {
    name: FormControl<string>;
    values: FormControl<string>;
    detailsPurpose: FormControl<string>;
    expectedBehaviorWhenSelected: FormControl<string>;
};

export function createModifierRow(fb: NonNullableFormBuilder): FormGroup<ModifierRowForm> {
    return fb.group<ModifierRowForm>({
        name: fb.control(''),
        values: fb.control('', { validators: [maxCommaSeparatedEntries(10)] }),
        detailsPurpose: fb.control(''),
        expectedBehaviorWhenSelected: fb.control(''),
    });
}

export type CustomerOnboardingForm = {
    existingClient: FormControl<boolean>;
    federatedAccessSSO: FormControl<boolean>;
    customerGroupName: FormControl<string>;
    customerAccountName: FormControl<string>;
    codaCode: FormControl<string>;
    validate: FormControl<boolean>;
    engage: FormControl<boolean>;
    insights: FormControl<boolean>;
    apps: FormControl<boolean>;
    msp: FormControl<boolean>;
    primaryUserFirstName: FormControl<string>;
    primaryUserLastName: FormControl<string>;
    primaryUserEmailAddress: FormControl<string>;
    primaryUserPreferredLanguage: FormControl<string>;
    otherUsers: FormArray<FormGroup<OtherUserRowForm>>;
    customizedTemplates: FormArray<FormGroup<CustomizedTemplateRowForm>>;
    businessModifiers: FormArray<FormGroup<ModifierRowForm>>;
    financeModifiers: FormArray<FormGroup<ModifierRowForm>>;
    processModifiers: FormArray<FormGroup<ModifierRowForm>>;
    considerAnythingElse : FormControl<string>;
    considerOperationalOwnerAccount : FormControl<string>;
    considerAccountManager : FormControl<string>;
    considerMigrationPoc : FormControl<string>;
}

const req = [Validators.required];

export function createCustomerOnboardingForm(fb: NonNullableFormBuilder): FormGroup<CustomerOnboardingForm> {
    return fb.group<CustomerOnboardingForm>({
        existingClient: fb.control(false),
        federatedAccessSSO: fb.control(false),
        customerGroupName: fb.control('', { validators: req }),
        customerAccountName: fb.control('', { validators: req }),
        codaCode: fb.control('', { validators: req }),
        validate: fb.control(false),
        engage: fb.control(false),
        insights: fb.control(false),
        apps: fb.control(false),
        msp: fb.control(false),
        primaryUserFirstName: fb.control('', { validators: req }),
        primaryUserLastName: fb.control('', { validators: req }),
        primaryUserEmailAddress: fb.control('', { validators: [Validators.required, Validators.email] }),
        primaryUserPreferredLanguage: fb.control('', { validators: req }),
        otherUsers: fb.array<FormGroup<OtherUserRowForm>>([]),
        customizedTemplates: fb.array<FormGroup<CustomizedTemplateRowForm>>([]),
        businessModifiers: fb.array<FormGroup<ModifierRowForm>>([]),
        financeModifiers: fb.array<FormGroup<ModifierRowForm>>([]),
        processModifiers: fb.array<FormGroup<ModifierRowForm>>([]),
        considerAnythingElse: fb.control(''),
        considerOperationalOwnerAccount: fb.control('', { validators: req }),
        considerAccountManager: fb.control('', { validators: req }),
        considerMigrationPoc: fb.control('', { validators: req }),
    });
}