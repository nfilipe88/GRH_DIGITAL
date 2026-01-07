import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmissaoDeclaracoes } from './emissao-declaracoes';

describe('EmissaoDeclaracoes', () => {
  let component: EmissaoDeclaracoes;
  let fixture: ComponentFixture<EmissaoDeclaracoes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmissaoDeclaracoes]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmissaoDeclaracoes);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
